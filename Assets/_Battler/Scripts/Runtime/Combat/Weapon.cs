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
