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
        private TrailRenderer trail;
        private static Material trailMaterial;

        /// <summary>
        /// A smear behind the striking part while a blow is in its strike and follow-through, so the
        /// arc reads at a distance (in play only).
        /// </summary>
        public void ShowTrail(bool on)
        {
            if (trail == null)
            {
                if (!on || !Application.isPlaying || definition == null || definition.IsShield)
                {
                    return;
                }

                var holder = new GameObject("Trail");
                holder.transform.SetParent(transform, false);
                holder.transform.localPosition = definition.tip;
                trail = holder.AddComponent<TrailRenderer>();
                trail.time = 0.16f;
                trail.startWidth = 0.09f * Scale;
                trail.endWidth = 0f;
                trail.minVertexDistance = 0.02f;
                trail.startColor = new Color(1f, 0.95f, 0.8f, 0.55f);
                trail.endColor = new Color(1f, 0.85f, 0.6f, 0f);
                trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                if (trailMaterial == null)
                {
                    var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
                    trailMaterial = shader != null ? new Material(shader) : null;
                }

                trail.sharedMaterial = trailMaterial;
            }

            trail.emitting = on;
        }

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
    }
}
