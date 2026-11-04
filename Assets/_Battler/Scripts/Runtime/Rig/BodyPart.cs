using PhysicsBattler.Muscles;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Rig
{
    /// <summary>
    /// One rigid body part of a unit: its rigidbody, its joint to the parent part and its collider.
    /// Lives on the part's null (whose origin is the joint). Collisions on the part (and on a held
    /// weapon, whose colliders join the hand's rigidbody) are passed to the owning unit.
    /// </summary>
    [DisallowMultipleComponent]
    public class BodyPart : MonoBehaviour
    {
        public PartId id;
        public Rigidbody body;
        public ConfigurableJoint joint;
        public Collider partCollider;
        /// <summary>The limits PhysX holds, in the joint's frame (<see cref="jointFrame"/>).</summary>
        public JointLimitSpec limits;

        /// <summary>Milestone 10: the joint's frame in the part's rest frame (<see cref="JointFrames"/>; set up at start).</summary>
        [System.NonSerialized] public Quaternion jointFrame = Quaternion.identity;

        /// <summary>The joint's turn now (physical; the rest taken out) in the joint's frame, as PhysX limits it.</summary>
        public Quaternion JointTurn(Quaternion parentRotation, Quaternion rotation) =>
            JointFrames.ToJoint(Quaternion.Inverse(restLocalRotation) * (Quaternion.Inverse(parentRotation) * rotation), jointFrame);
        public Vector3 restLocalPosition;
        public Quaternion restLocalRotation = Quaternion.identity;

        public UnitController Owner { get; set; }

        // Velocities recorded at the start of each physics step, before the solver changes them:
        // collision callbacks arrive after the step, when the contact has already slowed things down.
        public Vector3 CachedVelocity { get; private set; }
        public Vector3 CachedAngularVelocity { get; private set; }
        public Vector3 CachedCenterOfMass { get; private set; }

        /// <summary>Time.time of the last physics step this part touched the ground (or anything else).</summary>
        public float LastGroundContactTime { get; private set; } = -1f;
        public float LastOtherContactTime { get; private set; } = -1f;
        public Collider LastOtherContact { get; private set; }

        /// <summary>Time.time this part last touched its own unit (another of its parts, or its own weapon).</summary>
        public float LastSelfContactTime { get; private set; } = -1f;

        private static int groundLayer = -2;

        /// <summary>
        /// Where a point attached to this part (given where the transform shows it) is in the physics
        /// pose. Transforms of interpolated bodies lag the simulation inside FixedUpdate; the rigidbody
        /// doesn't.
        /// </summary>
        public Vector3 PhysicalPoint(Vector3 pointOnTransform)
        {
            Vector3 local = Quaternion.Inverse(transform.rotation) * (pointOnTransform - transform.position);
            return body.position + body.rotation * local;
        }

        public void CacheState()
        {
            CachedVelocity = body.linearVelocity;
            CachedAngularVelocity = body.angularVelocity;
            CachedCenterOfMass = body.worldCenterOfMass;
        }

        /// <summary>The velocity of a world point on this part as it was at the start of the step.</summary>
        public Vector3 CachedPointVelocity(Vector3 worldPoint)
        {
            return CachedVelocity + Vector3.Cross(CachedAngularVelocity, worldPoint - CachedCenterOfMass);
        }

        private void RecordContact(Collision collision)
        {
            var other = collision.collider;
            if (other == null)
            {
                return;
            }

            if (groundLayer == -2)
            {
                groundLayer = LayerMask.NameToLayer(Layers.Ground);
            }

            if (other.gameObject.layer == groundLayer)
            {
                LastGroundContactTime = Time.time;
            }
            else if (Owner != null && IsOwn(other))
            {
                LastSelfContactTime = Time.time;
            }
            else
            {
                LastOtherContactTime = Time.time;
                LastOtherContact = other;
            }
        }

        private bool IsOwn(Collider other)
        {
            var part = other.GetComponentInParent<BodyPart>();
            return part != null && part.Owner == Owner;
        }

        /// <summary>A part (any unit's) struck something: for the sounds of bodies on the ground.</summary>
        public static event System.Action<BodyPart, Collision> Collided;

        /// <summary>The ground's layer (Layers.Ground), looked up once.</summary>
        public static int GroundLayer
        {
            get
            {
                if (groundLayer == -2)
                {
                    groundLayer = LayerMask.NameToLayer(Layers.Ground);
                }

                return groundLayer;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            RecordContact(collision);
            Collided?.Invoke(this, collision);
            if (Owner != null)
            {
                Owner.OnPartCollision(this, collision, true);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            RecordContact(collision);
            // Only the hands report contact every step (a weapon pressing into a body keeps hitting).
            if (Owner != null && PartInfo.IsHand(id))
            {
                Owner.OnPartCollision(this, collision, false);
            }
        }
    }
}
