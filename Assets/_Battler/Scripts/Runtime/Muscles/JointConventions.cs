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

        /// <summary>
        /// Per joint axis (X, Y, Z): +1 if targetAngularVelocity turns the body that way in its own frame,
        /// -1 if mirrored. Zero (and <see cref="VelocityProbed"/> false) if the probe saw no motion.
        /// </summary>
        public static Vector3 VelocitySign { get; private set; } = Vector3.zero;

        public static bool VelocityProbed { get; private set; }

        /// <summary>A one-line summary of what was measured, for the log and the sandbox panel.</summary>
        public static string Report { get; private set; } = "not probed";

        private const float TestAngle = 35f;
        private const float Step = 1f / 60f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            DriveInverted = false;
            TwistLimitSign = 1;
            VelocitySign = Vector3.zero;
            VelocityProbed = false;
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

                // 3. Target speed: a damper-only drive asked to turn +1.5 rad/s about each axis in turn.
                var sign = Vector3.zero;
                var turned = Vector3.zero;
                for (int axis = 0; axis < 3; axis++)
                {
                    var wanted = Vector3.zero;
                    wanted[axis] = 1.5f;
                    var rotation = RunRotation(scene, physics, joint =>
                    {
                        joint.rotationDriveMode = RotationDriveMode.Slerp;
                        joint.slerpDrive = new JointDrive { positionSpring = 0f, positionDamper = 300f, maximumForce = float.MaxValue, useAcceleration = true };
                        joint.targetAngularVelocity = wanted;
                        joint.angularYLimit = new SoftJointLimit { limit = 80f };
                        joint.angularZLimit = new SoftJointLimit { limit = 80f };
                    }, 18);
                    Vector3 rotated = JointMath.BodyAngularVelocity(Quaternion.identity, rotation, 1f);   // axis × angle
                    turned[axis] = rotated[axis] * Mathf.Rad2Deg;
                    sign[axis] = Mathf.Abs(turned[axis]) > 4f ? Mathf.Sign(turned[axis]) : 0f;
                }

                VelocitySign = sign;
                VelocityProbed = sign.x != 0f && sign.y != 0f && sign.z != 0f;

                Report = $"drive {(DriveInverted ? "inverted" : "standard")} (asked +{TestAngle:0}, got {driven:0.0}), " +
                         $"X limit sign {TwistLimitSign:+0;-0} (pushed +, stopped at {limited:0.0}), " +
                         $"velocity signs {sign.x:+0;-0;0} {sign.y:+0;-0;0} {sign.z:+0;-0;0} (turned {turned.x:0} {turned.y:0} {turned.z:0}°" +
                         $"{(VelocityProbed ? "" : "; speed feed-forward OFF")})";
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
            return JointMath.PitchOf(RunRotation(scene, physics, configure, 240, push));
        }

        /// <summary>The same rig; returns the child's rotation relative to the parent after <paramref name="steps"/> steps.</summary>
        private static Quaternion RunRotation(Scene scene, PhysicsScene physics, System.Action<ConfigurableJoint> configure,
            int steps, System.Action<Rigidbody> push = null)
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

            for (int i = 0; i < steps; i++)
            {
                push?.Invoke(childBody);
                physics.Simulate(Step);
            }

            var relative = Quaternion.Inverse(parentBody.rotation) * childBody.rotation;

            // Immediately, so the next run's boxes don't share the scene with these.
            Object.DestroyImmediate(childObject);
            Object.DestroyImmediate(parentObject);
            return relative;
        }
    }
}
