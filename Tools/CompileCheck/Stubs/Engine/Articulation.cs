// Unity's ArticulationBody, as much of it as the game uses: the data a script sets and reads. Without an engine under
// the stubs it does nothing; with one (the headless PhysX world) each root and what hangs from it is one articulation,
// built when the engine first steps after it was set up.
//
// Taken to be as Unity has it (the game's own probe measures the real thing in the editor and says so in the log):
// a drive's target is in degrees and its speed in degrees a second; its stiffness and damping are for radians; its
// force limit is a torque; a ball joint's three targets are its turn vector (axis times angle) on the anchor's X, Y, Z.
using System.Collections.Generic;

namespace UnityEngine
{
    public enum ArticulationJointType { FixedJoint = 0, PrismaticJoint = 1, RevoluteJoint = 2, SphericalJoint = 3 }
    public enum ArticulationDofLock { LockedMotion = 0, LimitedMotion = 1, FreeMotion = 2 }
    public enum ArticulationDriveAxis { X = 0, Y = 1, Z = 2 }
    public enum ArticulationDriveType { Force = 0, Acceleration = 1, Target = 2, Velocity = 3 }

    public struct ArticulationDrive
    {
        public float lowerLimit, upperLimit, stiffness, damping, forceLimit, target, targetVelocity;
        public ArticulationDriveType driveType;
    }

    public struct ArticulationReducedSpace
    {
        public int dofCount;
        private float a, b, c;
        public ArticulationReducedSpace(float x, float y, float z) { a = x; b = y; c = z; dofCount = 3; }
        public float this[int i]
        {
            get => i == 0 ? a : i == 1 ? b : c;
            set { if (i == 0) a = value; else if (i == 1) b = value; else c = value; }
        }
    }

    public class ArticulationBody : Behaviour
    {
        /// <summary>A push asked for since the engine last stepped (the engine under the stubs gives it).</summary>
        public struct Pending { public Vector3 Force, At; public ForceMode Mode; public int Kind; }

        public readonly List<Pending> Pushes = new List<Pending>();

        /// <summary>The engine's own handle for this link (the engine under the stubs keeps it).</summary>
        public object Engine;

        /// <summary>Its root was asked to be put somewhere at once (the engine under the stubs does it).</summary>
        public bool Teleported;
        public Vector3 TeleportTo;
        public Quaternion TeleportTurn;

        public float mass { get; set; } = 1f;
        public bool useGravity { get; set; } = true;
        public bool immovable { get; set; }
        public bool isRoot => transform.parent == null || transform.parent.GetComponentInParent<ArticulationBody>() == null;
        public bool automaticCenterOfMass { get; set; } = true;
        public bool automaticInertiaTensor { get; set; } = true;
        public Vector3 centerOfMass { get; set; }
        public Vector3 inertiaTensor { get; set; } = Vector3.one * 0.1f;
        public Quaternion inertiaTensorRotation { get; set; } = Quaternion.identity;
        public Vector3 worldCenterOfMass => transform.TransformPoint(centerOfMass);
        public ArticulationJointType jointType { get; set; }
        public Vector3 anchorPosition { get; set; }
        public Quaternion anchorRotation { get; set; } = Quaternion.identity;
        public Vector3 parentAnchorPosition { get; set; }
        public Quaternion parentAnchorRotation { get; set; } = Quaternion.identity;
        public bool matchAnchors { get; set; } = true;
        public ArticulationDofLock twistLock { get; set; } = ArticulationDofLock.FreeMotion;
        public ArticulationDofLock swingYLock { get; set; } = ArticulationDofLock.FreeMotion;
        public ArticulationDofLock swingZLock { get; set; } = ArticulationDofLock.FreeMotion;
        public ArticulationDofLock linearLockX { get; set; }
        public ArticulationDofLock linearLockY { get; set; }
        public ArticulationDofLock linearLockZ { get; set; }
        public ArticulationDrive xDrive { get => drives[0]; set { drives[0] = value; DrivesChanged = true; } }
        public ArticulationDrive yDrive { get => drives[1]; set { drives[1] = value; DrivesChanged = true; } }
        public ArticulationDrive zDrive { get => drives[2]; set { drives[2] = value; DrivesChanged = true; } }

        /// <summary>Its three drives as they stand (the engine under the stubs reads them).</summary>
        public readonly ArticulationDrive[] drives = new ArticulationDrive[3];

        /// <summary>A drive's stiffness, damping or limit was set since the engine last looked.</summary>
        public bool DrivesChanged = true;

        public void SetDriveTarget(ArticulationDriveAxis axis, float value) => drives[(int)axis].target = value;
        public void SetDriveTargetVelocity(ArticulationDriveAxis axis, float value) => drives[(int)axis].targetVelocity = value;
        public void SetDriveStiffness(ArticulationDriveAxis axis, float value) { drives[(int)axis].stiffness = value; DrivesChanged = true; }
        public void SetDriveDamping(ArticulationDriveAxis axis, float value) { drives[(int)axis].damping = value; DrivesChanged = true; }
        public void SetDriveForceLimit(ArticulationDriveAxis axis, float value) { drives[(int)axis].forceLimit = value; DrivesChanged = true; }

        public float linearDamping { get; set; } = 0.05f;
        public float angularDamping { get; set; } = 0.05f;
        public float jointFriction { get; set; } = 0.05f;
        public float maxJointVelocity { get; set; } = 100f;
        public float maxAngularVelocity { get; set; } = 50f;
        public float maxLinearVelocity { get; set; } = 100f;
        public float maxDepenetrationVelocity { get; set; } = 10f;
        public int solverIterations { get; set; } = 4;
        public int solverVelocityIterations { get; set; } = 1;
        public float sleepThreshold { get; set; } = 0.005f;
        public CollisionDetectionMode collisionDetectionMode { get; set; }
        public Vector3 linearVelocity { get; set; }
        public Vector3 angularVelocity { get; set; }
        private ArticulationReducedSpace position;

        /// <summary>Its joint's position (radians). Set, the joint is put there at once (the engine under the stubs does it).</summary>
        public ArticulationReducedSpace jointPosition { get => position; set { position = value; PositionAsked = true; } }

        /// <summary>Its joint's position as the engine has it now (the engine under the stubs tells it).</summary>
        public void Tell(ArticulationReducedSpace now) => position = now;

        public bool PositionAsked;
        public ArticulationReducedSpace jointVelocity { get; set; }
        public int dofCount => jointType == ArticulationJointType.SphericalJoint ? 3 : jointType == ArticulationJointType.FixedJoint ? 0 : 1;

        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force) => Pushes.Add(new Pending { Force = force, Mode = mode, Kind = 0 });
        public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force) => Pushes.Add(new Pending { Force = force, At = position, Mode = mode, Kind = 1 });
        public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force) => Pushes.Add(new Pending { Force = torque, Mode = mode, Kind = 2 });
        public Vector3 GetPointVelocity(Vector3 worldPoint) => linearVelocity + Vector3.Cross(angularVelocity, worldPoint - worldCenterOfMass);

        public void TeleportRoot(Vector3 position, Quaternion rotation)
        {
            Teleported = true;
            TeleportTo = position;
            TeleportTurn = rotation;
        }

        public void WakeUp() { }
        public void ResetCenterOfMass() { }
        public void ResetInertiaTensor() { }
    }
}
