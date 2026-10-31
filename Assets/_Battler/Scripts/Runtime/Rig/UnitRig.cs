using UnityEngine;

namespace PhysicsBattler.Rig
{
    /// <summary>
    /// The unit's physical rig: every body part by <see cref="PartId"/>, and the marker nulls
    /// (weapon slots and aim points). Filled in by <see cref="UnitBuilder"/> when the prefab is built.
    /// </summary>
    [DisallowMultipleComponent]
    public class UnitRig : MonoBehaviour
    {
        [SerializeField] private BodyPart[] parts = new BodyPart[PartInfo.Count];
        [SerializeField] private Transform slotWeaponR;
        [SerializeField] private Transform slotWeaponL;
        [SerializeField] private Transform aimChest;
        [SerializeField] private Transform aimHead;
        [SerializeField] private Transform aimNeck;
        [SerializeField] private Transform aimBelly;
        [SerializeField] private float scale = 1f;

        public Transform SlotWeaponR => slotWeaponR;
        public Transform SlotWeaponL => slotWeaponL;
        public Transform AimChest => aimChest;
        public Transform AimHead => aimHead;
        public Transform AimNeck => aimNeck;
        public Transform AimBelly => aimBelly;

        /// <summary>Size relative to the rig file (the goblin is a 0.62 orc).</summary>
        public float Scale => scale;

        public BodyPart Part(PartId id) => parts[(int)id];

        public BodyPart[] Parts => parts;

        public void Initialize(BodyPart[] builtParts, Transform slotR, Transform slotL, Transform chestAim,
            Transform headAim, Transform neckAim, Transform bellyAim, float rigScale)
        {
            aimNeck = neckAim;
            aimBelly = bellyAim;
            parts = builtParts;
            slotWeaponR = slotR;
            slotWeaponL = slotL;
            aimChest = chestAim;
            aimHead = headAim;
            scale = rigScale;
        }

        public bool IsComplete()
        {
            if (parts == null || parts.Length != PartInfo.Count)
            {
                return false;
            }

            foreach (var part in parts)
            {
                if (part == null || part.body == null)
                {
                    return false;
                }
            }

            return slotWeaponR != null && aimChest != null;
        }

        /// <summary>Total mass of all parts (and anything their rigidbodies carry, like a weapon).</summary>
        public float TotalMass()
        {
            float mass = 0f;
            foreach (var part in parts)
            {
                mass += part.body.mass;
            }

            return mass;
        }

        /// <summary>Mass-weighted centre of all the parts.</summary>
        public Vector3 CenterOfMass()
        {
            Vector3 sum = Vector3.zero;
            float mass = 0f;
            foreach (var part in parts)
            {
                sum += part.body.worldCenterOfMass * part.body.mass;
                mass += part.body.mass;
            }

            return mass > 0f ? sum / mass : parts[0].transform.position;
        }

        /// <summary>Mass-weighted velocity of all the parts.</summary>
        public Vector3 CenterOfMassVelocity()
        {
            Vector3 sum = Vector3.zero;
            float mass = 0f;
            foreach (var part in parts)
            {
                sum += part.body.linearVelocity * part.body.mass;
                mass += part.body.mass;
            }

            return mass > 0f ? sum / mass : Vector3.zero;
        }

        public bool Owns(Collider collider)
        {
            if (collider == null)
            {
                return false;
            }

            var part = collider.GetComponentInParent<BodyPart>();
            return part != null && part.Owner != null && ReferenceEquals(part.Owner.Rig, this);
        }
    }
}
