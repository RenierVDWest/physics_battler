using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhysicsBattler.Muscles
{
    /// <summary>
    /// ConfigurableJoint has two sign conventions that are easy to get backwards: which way
    /// targetRotation turns the body, and which side of the twist (X) limit is "low". Rather than
    /// trusting memory, this measures both once per play session in a tiny private physics scene:
    /// two boxes, one joint, a few hundred steps. The results feed <see cref="JointMath"/>.
    /// </summary>
    public static class JointConventions
    {
        /// <summary>True when the standard targetRotation formula turned the body the wrong way.</summary>
        public static bool DriveInverted { get; private set; }

        /// <summary>+1 when the X limit's high value bounds positive local rotation, -1 when it is mirrored.</summary>
        public static int TwistLimitSign { get; private set; } = 1;

        public static bool Probed { get; private set; }

        /// <summary>A one-line summary of what was measured, for the log and the sandbox panel.</summary>
        public static string Report { get; private set; } = "not probed";

        private const float TestAngle = 35f;
        private const float Step = 1f / 60f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            DriveInverted = false;
            TwistLimitSign = 1;
            Probed = false;
            Report = "not probed";
        }

        public static void EnsureProbed()
        {
            if (Probed || !Application.isPlaying)
            {
                return;
            }

            Probed = true;
            var scene = SceneManager.CreateScene("JointConventionProbe", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            try
            {
                var physics = scene.GetPhysicsScene();

                // 1. Drive: ask for +35 degrees about X with the standard formula and see where it ends up.
                DriveInverted = false;
                float driven = Run(scene, physics, joint =>
                {
                    var drive = new JointDrive { positionSpring = 900f, positionDamper = 60f, maximumForce = float.MaxValue, useAcceleration = true };
                    joint.rotationDriveMode = RotationDriveMode.Slerp;
                    joint.slerpDrive = drive;
                    joint.targetRotation = JointMath.TargetRotation(Quaternion.AngleAxis(TestAngle, Vector3.right), Quaternion.identity);
                }, null);
                DriveInverted = driven < 0f;

                // 2. Limits: low -5, high +60 as written, then push the body hard toward positive X.
                TwistLimitSign = 1;
                float limited = Run(scene, physics, joint =>
                {
                    joint.lowAngularXLimit = new SoftJointLimit { limit = -5f };
                    joint.highAngularXLimit = new SoftJointLimit { limit = 60f };
                }, body => body.AddTorque(Vector3.right * 400f, ForceMode.Acceleration));
                TwistLimitSign = limited > 30f ? 1 : -1;

                Report = $"drive {(DriveInverted ? "inverted" : "standard")} (asked +{TestAngle:0}, got {driven:0.0}), " +
                         $"X limit sign {TwistLimitSign:+0;-0} (pushed +, stopped at {limited:0.0})";
                Debug.Log($"[Joint probe] {Report}");
            }
            finally
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        /// <summary>Builds a fixed parent and a jointed child, simulates, and returns the child's pitch.</summary>
        private static float Run(Scene scene, PhysicsScene physics, System.Action<ConfigurableJoint> configure,
            System.Action<Rigidbody> push)
        {
            var parentObject = new GameObject("ProbeParent");
            var childObject = new GameObject("ProbeChild");
            SceneManager.MoveGameObjectToScene(parentObject, scene);
            SceneManager.MoveGameObjectToScene(childObject, scene);
            parentObject.transform.position = new Vector3(0f, 10f, 0f);
            childObject.transform.position = new Vector3(0f, 10f, 0f);

            var parentBody = parentObject.AddComponent<Rigidbody>();
            parentBody.isKinematic = true;
            parentBody.useGravity = false;

            var box = childObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, -0.25f, 0f);
            box.size = new Vector3(0.1f, 0.5f, 0.1f);
            var childBody = childObject.AddComponent<Rigidbody>();
            childBody.useGravity = false;
            childBody.mass = 2f;

            var joint = childObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = parentBody;
            joint.anchor = Vector3.zero;
            joint.axis = Vector3.right;
            joint.secondaryAxis = Vector3.up;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            // Y and Z are Limited (not Locked) like the real rig's joints: PhysX ignores a slerp drive
            // when any angular axis is locked.
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;
            joint.lowAngularXLimit = new SoftJointLimit { limit = -170f };
            joint.highAngularXLimit = new SoftJointLimit { limit = 170f };
            joint.angularYLimit = new SoftJointLimit { limit = 1f };
            joint.angularZLimit = new SoftJointLimit { limit = 1f };
            configure(joint);

            for (int i = 0; i < 240; i++)
            {
                push?.Invoke(childBody);
                physics.Simulate(Step);
            }

            var relative = Quaternion.Inverse(parentBody.rotation) * childBody.rotation;
            float pitch = JointMath.PitchOf(relative);

            // Immediately, so the next run's boxes don't share the scene with these.
            Object.DestroyImmediate(childObject);
            Object.DestroyImmediate(parentObject);
            return pitch;
        }
    }
}
