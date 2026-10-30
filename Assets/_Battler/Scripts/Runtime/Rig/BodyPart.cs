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
        public JointLimitSpec limits;
        public Vector3 restLocalPosition;
        public Quaternion restLocalRotation = Quaternion.identity;

        public UnitController Owner { get; set; }

        // Velocities recorded at the start of each physics step, before the solver changes them:
        // collision callbacks arrive after the step, when the contact has already slowed things down.
        public Vector3 CachedVelocity { get; private set; }
        public Vector3 CachedAngularVelocity { get; private set; }
        public Vector3 CachedCenterOfMass { get; private set; }

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

        private void OnCollisionEnter(Collision collision)
        {
            if (Owner != null)
            {
                Owner.OnPartCollision(this, collision, true);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            // Only the feet need contact every step (to know the unit is standing on something),
            // and the hand (a weapon pressing into a body keeps hitting after the first contact).
            if (Owner != null && (PartInfo.IsFoot(id) || PartInfo.IsHand(id)))
            {
                Owner.OnPartCollision(this, collision, false);
            }
        }
    }
}
