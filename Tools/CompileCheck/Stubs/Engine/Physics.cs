// Compile-check stubs: the physics API surface this project uses (signatures checked against
// Unity 6000.0 reference source; Find APIs updated for 6000.6). No simulation.
using System;

namespace UnityEngine
{
    public enum ForceMode { Force = 0, Acceleration = 5, Impulse = 1, VelocityChange = 2 }
    public enum RigidbodyInterpolation { None, Interpolate, Extrapolate }
    public enum CollisionDetectionMode { Discrete, Continuous, ContinuousDynamic, ContinuousSpeculative }
    public enum QueryTriggerInteraction { UseGlobal, Ignore, Collide }
    public enum ConfigurableJointMotion { Locked, Limited, Free }
    public enum RotationDriveMode { XYAndZ, Slerp }
    public enum SimulationMode { FixedUpdate, Update, Script }
    public enum PhysicsMaterialCombine { Average = 0, Multiply = 1, Minimum = 2, Maximum = 3 }

    public class Rigidbody : Component
    {
        public float mass { get; set; } = 1f;
        public bool useGravity { get; set; } = true;
        public bool isKinematic { get; set; }
        public float linearDamping { get; set; }
        public float angularDamping { get; set; } = 0.05f;
        public RigidbodyInterpolation interpolation { get; set; }
        public CollisionDetectionMode collisionDetectionMode { get; set; }
        public int solverIterations { get; set; } = 6;
        public int solverVelocityIterations { get; set; } = 1;
        public float maxAngularVelocity { get; set; } = 7f;
        public float maxDepenetrationVelocity { get; set; } = 1e32f;
        public bool automaticCenterOfMass { get; set; } = true;
        public bool automaticInertiaTensor { get; set; } = true;
        public Vector3 linearVelocity { get; set; }
        public Vector3 angularVelocity { get; set; }
        public Vector3 centerOfMass { get; set; }
        public Vector3 worldCenterOfMass => position;
        public Vector3 inertiaTensor { get; set; } = Vector3.one * 0.1f;
        public Quaternion inertiaTensorRotation { get; set; } = Quaternion.identity;
        public Vector3 position { get => transform.position; set => transform.position = value; }
        public Quaternion rotation { get => transform.rotation; set => transform.rotation = value; }
        public LayerMask excludeLayers { get; set; }
        public LayerMask includeLayers { get; set; }

        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force) { }
        public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force) { }
        public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force) { }
        public void AddRelativeForce(Vector3 force, ForceMode mode = ForceMode.Force) { }
        public Vector3 GetPointVelocity(Vector3 worldPoint) => linearVelocity;
        public void ResetCenterOfMass() { }
        public void ResetInertiaTensor() { }
        public void MovePosition(Vector3 position) { }
        public void MoveRotation(Quaternion rotation) { }
        public void Sleep() { }
        public void WakeUp() { }
    }

    public class PhysicsMaterial : Object
    {
        public PhysicsMaterial() { }
        public PhysicsMaterial(string name) { this.name = name; }
        public float bounciness { get; set; }
        public float dynamicFriction { get; set; } = 0.6f;
        public float staticFriction { get; set; } = 0.6f;
        public PhysicsMaterialCombine frictionCombine { get; set; }
        public PhysicsMaterialCombine bounceCombine { get; set; }
    }

    public class Collider : Component
    {
        public bool enabled { get; set; } = true;
        public bool isTrigger { get; set; }
        public PhysicsMaterial sharedMaterial { get; set; }
        public PhysicsMaterial material { get; set; }
        public Rigidbody attachedRigidbody => GetComponentInParent<Rigidbody>();
        public Bounds bounds => default;
        public Vector3 ClosestPoint(Vector3 position) => position;
    }

    public struct Bounds
    {
        public Vector3 center;
        public Vector3 size;
    }

    public class BoxCollider : Collider
    {
        public Vector3 center { get; set; }
        public Vector3 size { get; set; } = Vector3.one;
    }

    public class SphereCollider : Collider
    {
        public Vector3 center { get; set; }
        public float radius { get; set; } = 0.5f;
    }

    public class CapsuleCollider : Collider
    {
        public Vector3 center { get; set; }
        public float radius { get; set; } = 0.5f;
        public float height { get; set; } = 2f;
        public int direction { get; set; } = 1;
    }

    public struct JointDrive
    {
        public float positionSpring { get; set; }
        public float positionDamper { get; set; }
        public float maximumForce { get; set; }
        public bool useAcceleration { get; set; }
    }

    // Not used by the project; declared so a project type with the same name shows up as ambiguous here, as in Unity.
    public struct JointLimits
    {
        public float min { get; set; }
        public float max { get; set; }
    }

    public struct SoftJointLimit
    {
        public float limit { get; set; }
        public float bounciness { get; set; }
        public float contactDistance { get; set; }
    }

    public struct SoftJointLimitSpring
    {
        public float spring { get; set; }
        public float damper { get; set; }
    }

    public class Joint : Component
    {
        public Rigidbody connectedBody { get; set; }
        public Vector3 axis { get; set; } = Vector3.right;
        public Vector3 anchor { get; set; }
        public Vector3 connectedAnchor { get; set; }
        public bool autoConfigureConnectedAnchor { get; set; } = true;
        public float breakForce { get; set; } = float.PositiveInfinity;
        public float breakTorque { get; set; } = float.PositiveInfinity;
        public bool enableCollision { get; set; }
        public bool enablePreprocessing { get; set; } = true;
        public float massScale { get; set; } = 1f;
        public float connectedMassScale { get; set; } = 1f;
        public Vector3 currentForce => Vector3.zero;
        public Vector3 currentTorque => Vector3.zero;
    }

    public class ConfigurableJoint : Joint
    {
        public Vector3 secondaryAxis { get; set; } = Vector3.up;
        public ConfigurableJointMotion xMotion { get; set; }
        public ConfigurableJointMotion yMotion { get; set; }
        public ConfigurableJointMotion zMotion { get; set; }
        public ConfigurableJointMotion angularXMotion { get; set; }
        public ConfigurableJointMotion angularYMotion { get; set; }
        public ConfigurableJointMotion angularZMotion { get; set; }
        public SoftJointLimit lowAngularXLimit { get; set; }
        public SoftJointLimit highAngularXLimit { get; set; }
        public SoftJointLimit angularYLimit { get; set; }
        public SoftJointLimit angularZLimit { get; set; }
        public SoftJointLimitSpring angularXLimitSpring { get; set; }
        public SoftJointLimitSpring angularYZLimitSpring { get; set; }
        public Quaternion targetRotation { get; set; } = Quaternion.identity;
        public Vector3 targetAngularVelocity { get; set; }
        public RotationDriveMode rotationDriveMode { get; set; }
        public JointDrive xDrive { get; set; }
        public JointDrive yDrive { get; set; }
        public JointDrive zDrive { get; set; }
        public Vector3 targetPosition { get; set; }
        public JointDrive angularXDrive { get; set; }
        public JointDrive angularYZDrive { get; set; }
        public JointDrive slerpDrive { get; set; }
        public bool configuredInWorldSpace { get; set; }
        public bool swapBodies { get; set; }
    }

    public struct ContactPoint
    {
        public Vector3 point { get; set; }
        public Vector3 normal { get; set; }
        public Vector3 impulse { get; set; }
        public Collider thisCollider { get; set; }
        public Collider otherCollider { get; set; }
        public float separation { get; set; }
    }

    public class Collision
    {
        public Vector3 impulse { get; set; }
        public Vector3 relativeVelocity { get; set; }
        public Rigidbody rigidbody { get; set; }
        public Component body => rigidbody;
        public Collider collider { get; set; }
        public Transform transform => collider != null ? collider.transform : null;
        public GameObject gameObject => collider != null ? collider.gameObject : null;
        public int contactCount { get; set; }
        public ContactPoint GetContact(int index) => default;
    }

    public struct RaycastHit
    {
        public Vector3 point { get; set; }
        public Vector3 normal { get; set; }
        public float distance { get; set; }
        public Collider collider { get; set; }
        public Rigidbody rigidbody => collider != null ? collider.attachedRigidbody : null;
        public Transform transform => collider != null ? collider.transform : null;
    }

    public static class Physics
    {
        public static Vector3 gravity { get; set; } = new Vector3(0f, -9.81f, 0f);
        public static float defaultContactOffset { get; set; } = 0.01f;
        public static float sleepThreshold { get; set; } = 0.005f;
        public static float bounceThreshold { get; set; } = 2f;
        public static float defaultMaxDepenetrationVelocity { get; set; } = 10f;
        public static int defaultSolverIterations { get; set; } = 6;
        public static int defaultSolverVelocityIterations { get; set; } = 1;
        public static float defaultMaxAngularSpeed { get; set; } = 50f;
        public static SimulationMode simulationMode { get; set; }
        public static bool autoSyncTransforms { get; set; }
        public static bool reuseCollisionCallbacks { get; set; } = true;

        public static void IgnoreCollision(Collider a, Collider b, bool ignore = true) { }
        public static void IgnoreLayerCollision(int layer1, int layer2, bool ignore = true) { }

        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance,
            int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            hitInfo = default;
            return false;
        }

        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)
        {
            hitInfo = default;
            return false;
        }

        public static bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance, int layerMask,
            QueryTriggerInteraction queryTriggerInteraction)
        {
            hitInfo = default;
            return false;
        }

        public static bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance)
        {
            hitInfo = default;
            return false;
        }

        public static void SyncTransforms() { }

        public static bool ComputePenetration(Collider colliderA, Vector3 positionA, Quaternion rotationA,
            Collider colliderB, Vector3 positionB, Quaternion rotationB, out Vector3 direction, out float distance)
        {
            direction = Vector3.zero;
            distance = 0f;
            return false;
        }
        public static void Simulate(float step) { }
    }

    public struct PhysicsScene
    {
        public bool IsValid() => true;
        public void Simulate(float step) { }
    }

    public static class PhysicsSceneExtensions
    {
        public static PhysicsScene GetPhysicsScene(this SceneManagement.Scene scene) => default;
    }
}
