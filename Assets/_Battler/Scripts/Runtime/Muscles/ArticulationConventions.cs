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

        /// <summary>
        /// A part built turned against its parent (a shield's grip, built as the shield is gripped): whether its
        /// joint's turn is taken on the part's own axes, from where it was built (so: its turn against its parent is
        /// how it was built, then the joint's turn). Otherwise on its parent's, before how it was built.
        /// </summary>
        public static bool TurnOnOwnAxes { get; private set; } = true;

        /// <summary>
        /// Whether the engine's own number for where a ball joint is (its jointPosition) reads as its targets do:
        /// the same turn vector, in radians. The body names each target the way nearest where its joint is, by
        /// this number; where it reads otherwise the links' own turns are used.
        /// </summary>
        public static bool PositionReadable { get; private set; }

        /// <summary>
        /// What a drive's target speed in degrees a second is multiplied by to give this engine the same (1: it
        /// takes degrees a second, as its targets are degrees; pi/180: radians a second), with its sign against
        /// the targets'. A joint off its pose is brought back by the speed its motor is asked for: fifty-seven
        /// times that, it would be thrown at its pose.
        /// </summary>
        public static float SpeedScale { get; private set; } = 1f;

        /// <summary>
        /// What a damping meant for radians a second is multiplied by, on top of <see cref="StiffnessScale"/>, to
        /// give this engine the same (1 where a damper holds a torque back as it should). A motor that does not
        /// damp as it is asked to never stops what it has once set swinging.
        /// </summary>
        public static float DampingScale { get; private set; } = 1f;

        /// <summary>
        /// Whether a ball joint's speed of turn (what a drive's target speed is compared with) is counted on the
        /// part's own axes: the turn that takes it on from where it is, taken after where it is. Otherwise on its
        /// parent's. The two are the same for a joint at rest and a quarter turn apart for one a quarter turn from
        /// it; sent on the wrong ones, an arm raised overhead is steered sideways of where it should go.
        /// </summary>
        public static bool SpeedOnOwnAxes { get; private set; } = true;

        public static string Report { get; private set; } = "not probed";

        private static Vector3 wentOwn, wentParents;

        private static Vector3 readAt;

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
                // (And where the engine says the joint then is: the same numbers, in radians?)
                float readOff = (Vector3.Scale(readAt * Mathf.Rad2Deg, sign) - Asked).magnitude;
                PositionReadable = turnVector && readOff < 3f;

                // 2. Stiffness: a steady 5 N m about X against a spring of 400 N m a radian gives 0.0125 rad.
                Vector3 held = Run(scene, physics, 400f, 12.65f, 0f, Vector3.zero, Vector3.right * 5f, 240);
                float gave = Mathf.Abs(held.x) * Mathf.Deg2Rad;
                float per = gave > 1e-6f ? 5f / (400f * gave) : 0f;
                StiffnessScale = per > 0.5f && per < 2f ? 1f : per > 20f && per < 120f ? 1f / per : 1f;
                bool stiffnessKnown = (per > 0.5f && per < 2f) || (per > 20f && per < 120f);

                // 3. The limit: the same torque against a drive that may only give 2 N m is not held.
                Vector3 slipped = Run(scene, physics, 400f * StiffnessScale, 12.65f * StiffnessScale, 2f, Vector3.zero, Vector3.right * 5f, 120);
                LimitIsTorque = Mathf.Abs(slipped.x) > 20f;

                // 4. A part built turned against its parent: the same turn asked of it, and which way it is taken.
                var built = Quaternion.Euler(0f, 70f, 30f);
                Vector3 turnedTo = Run(scene, physics, 400f, 12.65f, 0f, Vector3.Scale(Asked, sign), Vector3.zero, 240, built);
                var reached = JointMath.FromTurnVector(turnedTo * Mathf.Deg2Rad);
                var asked = JointMath.FromTurnVector(Asked * Mathf.Deg2Rad);
                float own = Quaternion.Angle(reached, built * asked), parents = Quaternion.Angle(reached, asked * built);
                TurnOnOwnAxes = own <= parents;
                bool builtKnown = Mathf.Min(own, parents) < 3f;

                // 5. A target speed: a damper alone asked for 40 degrees a second about X turns the part 4 degrees
                // in a tenth of a second, if degrees a second is what it takes (as radians a second: 229).
                Vector3 spun = Run(scene, physics, 0f, 50f * StiffnessScale, 0f, Vector3.zero, Vector3.zero, 24, null, new Vector3(40f * sign.x, 0f, 0f));
                float ran = Mathf.Abs(spun.x);
                bool speedKnown = (ran > 2.5f && ran < 5.5f) || ran > 60f;
                SpeedScale = (ran > 60f ? Mathf.Deg2Rad : 1f) * (spun.x < 0f ? -1f : 1f);

                // 6. Damping: a steady 5 N m about X against a damper of 5 N m s a radian turns at 1 rad/s: 13 degrees
                // in a quarter of a second.
                Vector3 dragged = Run(scene, physics, 0f, 5f * StiffnessScale, 0f, Vector3.zero, Vector3.right * 5f, 60);
                float drag = Mathf.Abs(dragged.x);
                bool dampingKnown = drag > 9f && drag < 18f;
                DampingScale = dampingKnown || drag < 0.05f || drag > 170f ? 1f : drag / 13.2f;

                // 7. Which axes a speed is on: the part turned a quarter round Y and held there, then let go and
                // asked for a speed about X alone. It turns about its own X, or about its parent's.
                Run(scene, physics, 400f * StiffnessScale, 12.65f * StiffnessScale * DampingScale, 0f, new Vector3(0f, 90f * sign.y, 0f), Vector3.zero, 240, null,
                    default, new Vector3(40f * sign.x * (SpeedScale < 0f ? -1f : 1f) * (Mathf.Abs(SpeedScale) < 0.5f ? Mathf.Deg2Rad : 1f), 0f, 0f));
                float offOwn = (wentOwn - new Vector3(4f, 0f, 0f)).magnitude, offParents = (wentParents - new Vector3(4f, 0f, 0f)).magnitude;
                bool axesKnown = Mathf.Min(offOwn, offParents) < 1.5f && Mathf.Abs(offOwn - offParents) > 1.5f;
                SpeedOnOwnAxes = !axesKnown || offOwn <= offParents;

                Usable = turnVector && stiffnessKnown;
                Report = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "targets {0} (asked {1:0} {2:0} {3:0}, turned {4:0.0} {5:0.0} {6:0.0}°, signs {7:+0;-0} {8:+0;-0} {9:+0;-0}), "
                    + "stiffness {10} (5 N m on 400 gave {11:0.000}°; for radians 0.716), force limit {12} (5 N m on a 2 N m limit turned {13:0}°), "
                    + "a part built turned takes its joint's turn {15} ({16:0.0}° off; the other way {17:0.0}°), "
                    + "the joint's own position reads {18} ({19:0.0}° off), a target speed is {20} (40 a second for 0.1 s turned {21:0.0}°), "
                    + "damping {22} (5 N m on a damper of 5 turned {23:0.0}° in 0.25 s; for radians a second 13.2), "
                    + "a speed is counted {24} (a quarter turn round Y, then 40 a second about X for 0.1 s: {25:0.0} {26:0.0} {27:0.0}° on its own axes, {28:0.0} {29:0.0} {30:0.0}° on its parent's){14}",
                    turnVector ? "a turn vector" : "NOT a turn vector", Asked.x, Asked.y, Asked.z, got.x, got.y, got.z, sign.x, sign.y, sign.z,
                    per > 0.5f && per < 2f ? "for radians" : stiffnessKnown ? "for degrees" : "UNKNOWN", Mathf.Abs(held.x),
                    LimitIsTorque ? "a torque" : "NOT a torque", Mathf.Abs(slipped.x), Usable ? "" : "; the physical upper body stays OFF",
                    !builtKnown ? "NEITHER way known" : TurnOnOwnAxes ? "on its own axes" : "on its parent's", Mathf.Min(own, parents), Mathf.Max(own, parents),
                    PositionReadable ? "as the targets do" : "OTHERWISE (the links' turns are used)", readOff,
                    !speedKnown ? "UNKNOWN (taken as degrees a second)" : Mathf.Abs(SpeedScale) < 0.5f ? "radians a second" : SpeedScale < 0f ? "degrees a second, the other way" : "degrees a second", spun.x,
                    dampingKnown ? "for radians a second" : Mathf.Abs(DampingScale - 1f) > 1e-3f ? $"OTHERWISE (ours is multiplied by {DampingScale:0.000})" : "UNKNOWN (left as it is)", drag,
                    !axesKnown ? "on axes UNKNOWN (taken as its own)" : SpeedOnOwnAxes ? "on the part's own axes" : "on its parent's axes",
                    wentOwn.x, wentOwn.y, wentOwn.z, wentParents.x, wentParents.y, wentParents.z);
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
        private static Vector3 Run(Scene scene, PhysicsScene physics, float stiffness, float damping, float limit, Vector3 target, Vector3 torque, int steps,
            Quaternion? built = null, Vector3 speed = default, Vector3 then = default)
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
            partObject.transform.localRotation = built ?? Quaternion.identity;
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
            drive.targetVelocity = speed.x;
            part.xDrive = drive;
            drive.target = target.y;
            drive.targetVelocity = speed.y;
            part.yDrive = drive;
            drive.target = target.z;
            drive.targetVelocity = speed.z;
            part.zDrive = drive;

            for (int i = 0; i < steps; i++)
            {
                if (torque.sqrMagnitude > 0f)
                {
                    part.AddTorque(torque, ForceMode.Force);
                }

                physics.Simulate(Step);
            }

            if (then != default)
            {
                // (Let go where it is, and asked for a speed alone: which way it turns from there.)
                var from = Quaternion.Inverse(rootObject.transform.rotation) * partObject.transform.rotation;
                drive.stiffness = 0f;
                drive.damping = 50f * StiffnessScale * DampingScale;
                drive.targetVelocity = then.x;
                part.xDrive = drive;
                drive.targetVelocity = then.y;
                part.yDrive = drive;
                drive.targetVelocity = then.z;
                part.zDrive = drive;
                for (int i = 0; i < 24; i++)
                {
                    physics.Simulate(Step);
                }

                var to = Quaternion.Inverse(rootObject.transform.rotation) * partObject.transform.rotation;
                wentOwn = Vector3.Scale(JointMath.TurnVector(Quaternion.Inverse(from) * to) * Mathf.Rad2Deg, TargetSign);
                wentParents = Vector3.Scale(JointMath.TurnVector(to * Quaternion.Inverse(from)) * Mathf.Rad2Deg, TargetSign);
            }

            var read = part.jointPosition;
            readAt = read.dofCount == 3 ? new Vector3(read[0], read[1], read[2]) : Vector3.one * 1e3f;
            var relative = Quaternion.Inverse(rootObject.transform.rotation) * partObject.transform.rotation;
            Object.DestroyImmediate(rootObject);
            return JointMath.TurnVector(relative) * Mathf.Rad2Deg;
        }
    }
}
