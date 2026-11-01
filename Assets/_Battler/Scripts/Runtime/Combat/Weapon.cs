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
        private bool physical;
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
            // Milestone 9c: a shield is solid (it stops against bodies, its contacts capped in
            // GroundContacts); weapons only report what they touch (solid only while their unit lies
            // on the ground: SetLying). All of it on the Weapon layer.
            bool solid = definition.IsShield && !mainHand && owner.IsPhysical;
            weapon.physical = owner.IsPhysical;
            int layer = Layers.Index(Layers.Weapon);
            foreach (var collider in weapon.Colliders)
            {
                collider.gameObject.layer = layer;
                collider.isTrigger = !solid;
                if (solid)
                {
                    GroundContacts.SetShield(collider, true);
                }
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

        /// <summary>
        /// Lets go of it (its hand was cut off): it is no one's any more. A physical unit's falls on its
        /// own: out of the hand, solid, with a body of its own carrying on as the hand moved, to land
        /// and come to rest on the floor (returned, to be thrown with the limb and cleared away with
        /// the unit). Otherwise it goes where the hand goes (null).
        /// </summary>
        public Rigidbody Release()
        {
            var from = hand != null ? hand.body : null;
            Owner = null;
            ShowTrail(false);
            SetLying(false);
            if (!physical)
            {
                return null;
            }

            Vector3 velocity = from != null ? from.GetPointVelocity(transform.position) : Vector3.zero;
            Vector3 spin = from != null ? from.angularVelocity : Vector3.zero;
            transform.SetParent(null, true);
            hand = null;
            var body = GetComponent<Rigidbody>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody>();
            }

            body.mass = Mathf.Max(0.1f, definition.massKg * Scale * Scale * Scale);
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;   // (a thin blade, turning)
            body.linearDamping = 0.05f;
            body.angularDamping = 0.5f;
            body.isKinematic = false;
            body.linearVelocity = velocity;
            body.angularVelocity = spin;
            foreach (var collider in Colliders)
            {
                collider.isTrigger = false;
                GroundContacts.SetShield(collider, false);
            }

            return body;
        }

        /// <summary>
        /// Milestone 9c: a held weapon is solid while its unit lies on the ground (alive or dead), so it
        /// rests on the floor and is pushed about as the body is (registered with GroundContacts as
        /// lying: a standing unit stepping on it nudges it, never flings it); standing again, it only
        /// reports what it touches. A shield is solid either way.
        /// </summary>
        public void SetLying(bool lying)
        {
            if (!physical || definition == null || definition.IsShield)
            {
                return;
            }

            lying &= Owner != null;
            if (lying == Lying)
            {
                return;
            }

            Lying = lying;
            float mass = definition.massKg * Scale * Scale * Scale;
            var list = new System.Collections.Generic.List<(Collider, float)>();
            foreach (var collider in Colliders)
            {
                collider.isTrigger = !lying;
                list.Add((collider, mass));
            }

            GroundContacts.Set(list, lying ? true : (bool?)null);
        }

        /// <summary>Solid on the ground with its unit (<see cref="SetLying"/>).</summary>
        public bool Lying { get; private set; }

        /// <summary>Its colliders.</summary>
        public Collider[] Colliders => GetComponentsInChildren<Collider>(true);

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
