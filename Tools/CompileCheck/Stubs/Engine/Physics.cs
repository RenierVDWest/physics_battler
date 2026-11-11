// Compile-check stubs: the physics API surface this project uses (signatures checked against
// Unity 6000.0 reference source; Find APIs updated for 6000.6). No simulation.
using System;
using System.Collections.Generic;

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

    /// <summary>
    /// A real physics engine under the stubs (the plan "A different approach", step 1: PhysX through MagicPhysX, set by
    /// the runner). Null: the dry run as before (no collisions, no joints; the runner's integrator moves the parts).
    /// </summary>
    public interface IPhysicsBackend
    {
        void AddForce(Rigidbody body, Vector3 force, ForceMode mode);
        void AddForceAtPosition(Rigidbody body, Vector3 force, Vector3 position, ForceMode mode);
        void AddTorque(Rigidbody body, Vector3 torque, ForceMode mode);
        Vector3 PointVelocity(Rigidbody body, Vector3 worldPoint);
        Vector3 InertiaTensor(Rigidbody body);
        Vector3 WorldCenterOfMass(Rigidbody body);
        void IgnoreCollision(Collider a, Collider b, bool ignore);
        void IgnoreLayerCollision(int a, int b, bool ignore);
        bool ComputePenetration(Collider a, Vector3 positionA, Quaternion rotationA, Collider b, Vector3 positionB, Quaternion rotationB, out Vector3 direction, out float distance);
        void Simulate(float step);
    }

    public class Rigidbody : Component
    {
        /// <summary>Moved with MovePosition or MoveRotation since the last simulated step (a kinematic body then pushes what it meets; set by position alone it is placed).</summary>
        public bool MovedThisStep;

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
        public Vector3 worldCenterOfMass => Physics.Backend != null ? Physics.Backend.WorldCenterOfMass(this) : position;
        private Vector3 inertia = Vector3.one * 0.1f;
        public Vector3 inertiaTensor { get => Physics.Backend != null ? Physics.Backend.InertiaTensor(this) : inertia; set => inertia = value; }
        public Quaternion inertiaTensorRotation { get; set; } = Quaternion.identity;
        public Vector3 position { get => transform.position; set => transform.position = value; }
        public Quaternion rotation { get => transform.rotation; set => transform.rotation = value; }
        public LayerMask excludeLayers { get; set; }
        public LayerMask includeLayers { get; set; }

        // Impulses change the velocity (roughly: the dry run has no inertia tensors); forces are ignored.
        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            if (isKinematic) return;
            if (Physics.Backend != null) { Physics.Backend.AddForce(this, force, mode); return; }
            if (mode == ForceMode.Impulse) linearVelocity += force / Mathf.Max(1e-3f, mass);
            else if (mode == ForceMode.VelocityChange) linearVelocity += force;
        }

        public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force)
        {
            if (Physics.Backend != null) { if (!isKinematic) Physics.Backend.AddForceAtPosition(this, force, position, mode); return; }
            if (isKinematic || mode != ForceMode.Impulse) return;
            linearVelocity += force / Mathf.Max(1e-3f, mass);
            angularVelocity += Vector3.Cross(position - this.position, force) / Mathf.Max(1e-3f, mass * 0.02f);
        }
        public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force) { if (Physics.Backend != null && !isKinematic) Physics.Backend.AddTorque(this, torque, mode); }
        public void AddRelativeForce(Vector3 force, ForceMode mode = ForceMode.Force) { }
        public Vector3 GetPointVelocity(Vector3 worldPoint) => Physics.Backend != null ? Physics.Backend.PointVelocity(this, worldPoint) : linearVelocity;
        public void ResetCenterOfMass() { }
        public void ResetInertiaTensor() { }
        // The stub has no solver: a move arrives at once (the smoke runner integrates the rest).
        public void MovePosition(Vector3 position) { this.position = position; MovedThisStep = true; }
        public void MoveRotation(Quaternion rotation) { this.rotation = rotation; MovedThisStep = true; }
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
        public bool hasModifiableContacts { get; set; }
        public PhysicsMaterial sharedMaterial { get; set; }
        public PhysicsMaterial material { get; set; }
        public Rigidbody attachedRigidbody => GetComponentInParent<Rigidbody>();
        public Bounds bounds => default;
        // Real geometry for the shapes the project uses (Unity's own is exact; this is close enough for the dry run).
        public Vector3 ClosestPoint(Vector3 position)
        {
            var t = transform;
            Vector3 local = Quaternion.Inverse(t.rotation) * (position - t.position);
            Vector3 s = t.lossyScale;
            local = new Vector3(local.x / s.x, local.y / s.y, local.z / s.z);
            Vector3 c = local;
            switch (this)
            {
                case BoxCollider box:
                {
                    Vector3 h = box.size * 0.5f;
                    Vector3 d = local - box.center;
                    c = box.center + new Vector3(Mathf.Clamp(d.x, -h.x, h.x), Mathf.Clamp(d.y, -h.y, h.y), Mathf.Clamp(d.z, -h.z, h.z));
                    break;
                }
                case SphereCollider sphere:
                {
                    Vector3 d = local - sphere.center;
                    c = d.magnitude <= sphere.radius ? local : sphere.center + d.normalized * sphere.radius;
                    break;
                }
                case CapsuleCollider capsule:
                {
                    Vector3 axis = capsule.direction == 0 ? Vector3.right : capsule.direction == 2 ? Vector3.forward : Vector3.up;
                    float half = Mathf.Max(0f, capsule.height * 0.5f - capsule.radius);
                    Vector3 a = capsule.center - axis * half, b = capsule.center + axis * half;
                    Vector3 ab = b - a;
                    float k = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector3.Dot(local - a, ab) / ab.sqrMagnitude) : 0f;
                    Vector3 onAxis = a + ab * k;
                    Vector3 d = local - onAxis;
                    c = d.magnitude <= capsule.radius ? local : onAxis + d.normalized * capsule.radius;
                    break;
                }
            }

            return t.position + t.rotation * new Vector3(c.x * s.x, c.y * s.y, c.z * s.z);
        }
    }

    public struct Bounds
    {
        public Vector3 center;
        public Vector3 size;
        public Vector3 extents => size * 0.5f;
        public Vector3 min => center - size * 0.5f;
        public Vector3 max => center + size * 0.5f;
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
        /// <summary>Destroyed (Object.Destroy on the component): the bodies it held are free of each other.</summary>
        public bool Destroyed;

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
        public ContactPoint[] Points;
        public int contactCount { get => Points != null ? Points.Length : count; set => count = value; }
        private int count;
        public ContactPoint GetContact(int index) => Points != null && index >= 0 && index < Points.Length ? Points[index] : default;
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

        /// <summary>The engine under the stubs (null: none, the dry run).</summary>
        public static IPhysicsBackend Backend;

        /// <summary>Every live rigidbody and joint, for the engine under the stubs.</summary>
        public static readonly List<Rigidbody> Bodies = new List<Rigidbody>();
        public static readonly List<Joint> Joints = new List<Joint>();
        public static IReadOnlyList<Collider> Colliders => colliders;

        public static void IgnoreCollision(Collider a, Collider b, bool ignore = true) { Backend?.IgnoreCollision(a, b, ignore); }
        public static event System.Action<PhysicsScene, Unity.Collections.NativeArray<ModifiableContactPair>> ContactModifyEvent { add { } remove { } }

        // Overlap queries over every live collider, with real shapes (Collider.ClosestPoint).
        private static readonly List<Collider> colliders = new List<Collider>();
        internal static void Register(Collider c) => colliders.Add(c);
        internal static void Unregister(Collider c) => colliders.Remove(c);

        public static Collider[] OverlapCapsule(Vector3 point0, Vector3 point1, float radius, int layerMask = -1,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            var found = new List<Collider>();
            foreach (var c in colliders)
            {
                if (!c.enabled || ((1 << c.gameObject.layer) & layerMask) == 0) continue;
                if (c.isTrigger && queryTriggerInteraction == QueryTriggerInteraction.Ignore) continue;
                for (int i = 0; i <= 8; i++)
                {
                    Vector3 p = Vector3.Lerp(point0, point1, i / 8f);
                    if (Vector3.Distance(c.ClosestPoint(p), p) <= radius)
                    {
                        found.Add(c);
                        break;
                    }
                }
            }

            return found.ToArray();
        }

        public static Collider[] OverlapSphere(Vector3 position, float radius, int layerMask = -1,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) =>
            OverlapCapsule(position, position, radius, layerMask, queryTriggerInteraction);
        public static void IgnoreLayerCollision(int layer1, int layer2, bool ignore = true) { Backend?.IgnoreLayerCollision(layer1, layer2, ignore); }

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
            return Backend != null && Backend.ComputePenetration(colliderA, positionA, rotationA, colliderB, positionB, rotationB, out direction, out distance);
        }
        public static void Simulate(float step) { Backend?.Simulate(step); }
    }

    public struct ModifiableContactPair
    {
        public EntityId colliderEntityId => default;
        public EntityId otherColliderEntityId => default;
        public EntityId bodyEntityId => default;
        public EntityId otherBodyEntityId => default;
        public int contactCount => 0;
        public float GetMaxImpulse(int i) => float.MaxValue;
        public void SetMaxImpulse(int i, float value) { }
        public void IgnoreContact(int i) { }
    }

    public struct PhysicsScene
    {
        public bool IsValid() => true;
        public void Simulate(float step) { Physics.Backend?.Simulate(step); }
    }

    public static class PhysicsSceneExtensions
    {
        public static PhysicsScene GetPhysicsScene(this SceneManagement.Scene scene) => default;
    }
}
