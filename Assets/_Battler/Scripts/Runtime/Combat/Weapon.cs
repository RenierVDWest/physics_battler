using System.Collections.Generic;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>
    /// A weapon or shield in a unit's hand: the mesh and colliders, fixed to the physical hand's
    /// weapon slot, so it moves as the pinned arm moves. In milestone 2 its colliders are triggers:
    /// it reports what it touches during a blow (who, which part, how fast) without pushing anything.
    /// Milestone 3 makes those contacts physical hits.
    /// </summary>
    [DisallowMultipleComponent]
    public class Weapon : MonoBehaviour
    {
        public WeaponDefinition definition;

        public UnitController Owner { get; private set; }
        public bool InMainHand { get; private set; }
        public float Scale { get; private set; } = 1f;

        private BodyPart hand;
        private readonly HashSet<BodyPart> touched = new HashSet<BodyPart>();

        /// <summary>
        /// Puts a weapon in a hand: the prefab (or, without one, a bare object with no mesh), at the
        /// slot, scaled with the unit.
        /// </summary>
        public static Weapon Equip(WeaponDefinition definition, UnitController owner, Transform slot, bool mainHand, float scale)
        {
            GameObject go;
            if (definition.prefab != null)
            {
                go = Instantiate(definition.prefab, slot);
                go.name = definition.prefab.name;
            }
            else
            {
                go = new GameObject("Weapon " + definition.name);
                go.transform.SetParent(slot, false);
            }

            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * scale;
            var weapon = go.GetComponent<Weapon>();
            if (weapon == null)
            {
                weapon = go.AddComponent<Weapon>();
            }

            weapon.definition = definition;
            weapon.Owner = owner;
            weapon.InMainHand = mainHand;
            weapon.Scale = scale;
            weapon.hand = owner.Rig.Part(mainHand ? PartId.HandR : PartId.HandL);
            foreach (var collider in go.GetComponentsInChildren<Collider>(true))
            {
                collider.isTrigger = true;
                foreach (var part in owner.Rig.Parts)
                {
                    if (part.partCollider != null)
                    {
                        Physics.IgnoreCollision(collider, part.partCollider, true);
                    }
                }
            }

            return weapon;
        }

        /// <summary>A point on the weapon (weapon axes, medium size) in the world.</summary>
        public Vector3 WorldPoint(Vector3 local) => transform.position + transform.rotation * (local * Scale);

        /// <summary>
        /// The same point where the physics simulation has it now (from the hand's rigidbody: inside
        /// FixedUpdate the transforms of interpolated bodies trail the simulation).
        /// </summary>
        public Vector3 PhysicalPoint(Vector3 local)
        {
            Vector3 world = WorldPoint(local);
            return hand != null && hand.body != null ? hand.PhysicalPoint(world) : world;
        }

        /// <summary>A new blow: every part may be reported once again.</summary>
        public void ResetTouches() => touched.Clear();

        // Enter and Stay: a part already inside the blade when the active window opens still counts.
        // (Triggers have no continuous detection: a very fast blade can pass a thin part between two
        // steps. Milestone 3's hits sweep the blade's path instead.)
        private void OnTriggerEnter(Collider other) => Touch(other);

        private void OnTriggerStay(Collider other) => Touch(other);

        private void Touch(Collider other)
        {
            if (Owner == null)
            {
                return;
            }

            var part = other.GetComponentInParent<BodyPart>();
            if (part == null || part.Owner == Owner || touched.Contains(part))
            {
                return;
            }

            if (Owner.OnWeaponTouched(this, part, other))
            {
                touched.Add(part);
            }
        }
    }
}
