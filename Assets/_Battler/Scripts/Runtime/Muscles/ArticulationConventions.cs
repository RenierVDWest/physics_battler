using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhysicsBattler.Muscles
{
    /// <summary>
    /// How this engine's articulations take what they are given, measured once at start-up as
    /// <see cref="JointConventions"/> measures the joints (the plan "A different approach", step 5): a small
    /// articulation in a physics scene of its own, a fixed root and one part on a ball joint. What the physical
    /// upper body (<see cref="PhysicsBattler.Authority.UpperBodyArticulation"/>) assumes, and what is checked:
    /// a ball joint's three drive targets are its turn against its parent as a turn vector (axis times angle, in
    /// degrees, on the part's own X, Y, Z); a drive's stiffness and damping are for radians; its force limit is a
    /// torque. The result is logged; where the engine differs the scales here are set so the body still holds.
    /// </summary>
    public static class ArticulationConventions
    {
        public static bool Probed { get; private set; }

        /// <summary>Each axis' drive target as the engine wants it, against the turn vector's (1 or -1).</summary>
        public static Vector3 TargetSign { get; private set; } = Vector3.one;

        /// <summary>What a stiffness or damping meant for radians is multiplied by to give this engine the same.</summary>
        public static float StiffnessScale { get; private set; } = 1f;

        /// <summary>Whether a drive's force limit holds a torque back as a torque limit would.</summary>
        public static bool LimitIsTorque { get; private set; } = true;

        /// <summary>Whether what was measured is what the body assumes (it can be built).</summary>
        public static bool Usable { get; private set; } = true;

        public static string Report { get; private set; } = "not probed";

        private const float Step = 1f / 240f;
        private static readonly Vector3 Asked = new Vector3(20f, -35f, 10f);

        public static void EnsureProbed()
        {
            if (Probed || !Application.isPlaying)
            {
                return;
            }

            Probed = true;
            var scene = SceneManager.CreateScene("ArticulationConventionProbe", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            try
            {
                var physics = scene.GetPhysicsScene();

                // 1. Targets: ask for a turn about all three axes at once and see where the part ends up.
                Vector3 got = Run(scene, physics, 400f, 12.65f, 0f, Asked, Vector3.zero, 240);
                var sign = new Vector3(Mathf.Sign(got.x * Asked.x), Mathf.Sign(got.y * Asked.y), Mathf.Sign(got.z * Asked.z));
                Vector3 mended = Vector3.Scale(got, sign);
                float off = (mended - Asked).magnitude;
                bool turnVector = off < 3f;
                TargetSign = sign;

                // 2. Stiffness: a steady 5 N m about X against a spring of 400 N m a radian gives 0.0125 rad.
                Vector3 held = Run(scene, physics, 400f, 12.65f, 0f, Vector3.zero, Vector3.right * 5f, 240);
                float gave = Mathf.Abs(held.x) * Mathf.Deg2Rad;
                float per = gave > 1e-6f ? 5f / (400f * gave) : 0f;
                StiffnessScale = per > 0.5f && per < 2f ? 1f : per > 20f && per < 120f ? 1f / per : 1f;
                bool stiffnessKnown = (per > 0.5f && per < 2f) || (per > 20f && per < 120f);

                // 3. The limit: the same torque against a drive that may only give 2 N m is not held.
                Vector3 slipped = Run(scene, physics, 400f * StiffnessScale, 12.65f * StiffnessScale, 2f, Vector3.zero, Vector3.right * 5f, 120);
                LimitIsTorque = Mathf.Abs(slipped.x) > 20f;

                Usable = turnVector && stiffnessKnown;
                Report = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "targets {0} (asked {1:0} {2:0} {3:0}, turned {4:0.0} {5:0.0} {6:0.0}°, signs {7:+0;-0} {8:+0;-0} {9:+0;-0}), "
                    + "stiffness {10} (5 N m on 400 gave {11:0.000}°; for radians 0.716), force limit {12} (5 N m on a 2 N m limit turned {13:0}°){14}",
                    turnVector ? "a turn vector" : "NOT a turn vector", Asked.x, Asked.y, Asked.z, got.x, got.y, got.z, sign.x, sign.y, sign.z,
                    per > 0.5f && per < 2f ? "for radians" : stiffnessKnown ? "for degrees" : "UNKNOWN", Mathf.Abs(held.x),
                    LimitIsTorque ? "a torque" : "NOT a torque", Mathf.Abs(slipped.x), Usable ? "" : "; the physical upper body stays OFF");
                Debug.Log($"[Articulation probe] {Report}");
            }
            finally
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        /// <summary>
        /// A fixed root and one part on a ball joint, its three drives alike; simulated, and the part's turn against
        /// the root returned as a turn vector (degrees).
        /// </summary>
        private static Vector3 Run(Scene scene, PhysicsScene physics, float stiffness, float damping, float limit, Vector3 target, Vector3 torque, int steps)
        {
            var rootObject = new GameObject("ArticulationProbeRoot");
            SceneManager.MoveGameObjectToScene(rootObject, scene);
            rootObject.transform.position = new Vector3(0f, 20f, 0f);
            var root = rootObject.AddComponent<ArticulationBody>();
            root.immovable = true;
            root.useGravity = false;
            var partObject = new GameObject("ArticulationProbePart");
            partObject.transform.SetParent(rootObject.transform, false);
            partObject.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            var part = partObject.AddComponent<ArticulationBody>();
            part.useGravity = false;
            part.jointType = ArticulationJointType.SphericalJoint;
            part.anchorPosition = Vector3.zero;
            part.anchorRotation = Quaternion.identity;
            part.twistLock = ArticulationDofLock.FreeMotion;
            part.swingYLock = ArticulationDofLock.FreeMotion;
            part.swingZLock = ArticulationDofLock.FreeMotion;
            part.mass = 1f;
            part.automaticCenterOfMass = false;
            part.automaticInertiaTensor = false;
            part.centerOfMass = Vector3.zero;
            part.inertiaTensor = Vector3.one * 0.1f;
            part.inertiaTensorRotation = Quaternion.identity;
            part.linearDamping = 0f;
            part.angularDamping = 0f;
            part.jointFriction = 0f;
            var drive = new ArticulationDrive
            {
                stiffness = stiffness, damping = damping, forceLimit = limit > 0f ? limit : float.MaxValue,
                lowerLimit = -180f, upperLimit = 180f,
            };
            drive.target = target.x;
            part.xDrive = drive;
            drive.target = target.y;
            part.yDrive = drive;
            drive.target = target.z;
            part.zDrive = drive;

            for (int i = 0; i < steps; i++)
            {
                if (torque.sqrMagnitude > 0f)
                {
                    part.AddTorque(torque, ForceMode.Force);
                }

                physics.Simulate(Step);
            }

            var relative = Quaternion.Inverse(rootObject.transform.rotation) * partObject.transform.rotation;
            Object.DestroyImmediate(rootObject);
            return JointMath.TurnVector(relative) * Mathf.Rad2Deg;
        }
    }
}
