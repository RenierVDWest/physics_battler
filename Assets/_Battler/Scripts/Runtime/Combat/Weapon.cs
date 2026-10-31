using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>
    /// A held weapon. It has no rigidbody of its own: when equipped it sits under the hand's weapon
    /// slot, so its colliders join the hand's rigidbody as one compound body (the most stable way to
    /// hold a long, heavy object in PhysX). Markers give the grip (the origin), sweet spot and tip.
    /// </summary>
    [DisallowMultipleComponent]
    public class Weapon : MonoBehaviour
    {
        public WeaponDefinition definition;
        public Transform sweetSpot;
        public Transform tip;
        public Collider[] colliders = new Collider[0];

        public UnitController Owner { get; private set; }
        public BodyPart Hand { get; private set; }
        public bool IsHeld => Owner != null;

        /// <summary>Grip to sweet spot, in the weapon's own (unscaled) units.</summary>
        public float SweetSpotDistance => sweetSpot != null ? sweetSpot.localPosition.magnitude : 1f;

        /// <summary>Grip to tip, in the weapon's own units.</summary>
        public float TipDistance => tip != null ? tip.localPosition.magnitude : SweetSpotDistance;

        public WeaponDefinition Definition => definition != null ? definition : (definition = WeaponDefinition.CreateDefault());

        public bool Owns(Collider collider)
        {
            foreach (var own in colliders)
            {
                if (own == collider)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Where the held weapon is in the physics pose (the hand's rigidbody, not its interpolated
        /// transform): the grip, the direction it points (grip to tip), and the tip.
        /// </summary>
        public bool PhysicalPose(out Vector3 grip, out Vector3 direction, out Vector3 tipPoint)
        {
            grip = direction = tipPoint = Vector3.zero;
            if (Hand == null)
            {
                return false;
            }

            Quaternion rotation = Hand.body.rotation * (Quaternion.Inverse(Hand.transform.rotation) * transform.rotation);
            grip = Hand.PhysicalPoint(transform.position);
            direction = rotation * Vector3.forward;
            Vector3 tipLocal = tip != null ? transform.InverseTransformPoint(tip.position) : Vector3.forward * TipDistance;
            tipPoint = grip + rotation * tipLocal;
            return true;
        }

        /// <summary>The tip's speed (m/s) in the physics pose.</summary>
        public float TipSpeed()
        {
            return PhysicalPose(out _, out _, out Vector3 tipPoint) ? Hand.body.GetPointVelocity(tipPoint).magnitude : 0f;
        }

        public void AttachTo(UnitController owner, BodyPart hand, Transform slot, int weaponLayer)
        {
            Owner = owner;
            Hand = hand;
            transform.SetParent(slot, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            foreach (var own in colliders)
            {
                if (own != null)
                {
                    own.gameObject.layer = weaponLayer;
                }
            }
        }
    }
}
