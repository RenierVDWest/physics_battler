using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// Milestone 9b: a standing unit can't fling a body lying on the ground. A standing unit's legs
    /// are kinematic (to PhysX infinitely heavy) and its upper body is driven hard to its pose; where
    /// one of its parts touches a lying body's part, the push is capped at <see cref="MaxNudge"/> of
    /// that part's weight each step, so a body is nudged and rolled, as a heavy one would be, never
    /// thrown. Everything else (the floor, weapons, other bodies on the ground) is left alone.
    ///
    /// PhysX hands the contacts over on its own threads during the step, so what is lying and what is
    /// standing is copied into a snapshot on the main thread whenever it changes.
    /// </summary>
    public static class GroundContacts
    {
        /// <summary>The most a standing unit's part may change a lying part's speed in one step (m/s).</summary>
        public const float MaxNudge = 0.2f;

        private sealed class Snapshot
        {
            public readonly Dictionary<EntityId, float> lying = new Dictionary<EntityId, float>();
            public readonly HashSet<EntityId> standing = new HashSet<EntityId>();
        }

        private static readonly Dictionary<Collider, float> Lying = new Dictionary<Collider, float>();
        private static readonly HashSet<Collider> Standing = new HashSet<Collider>();
        private static volatile Snapshot current;
        private static bool dirty, hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Lying.Clear();
            Standing.Clear();
            current = null;
            dirty = false;
            if (hooked)
            {
                Physics.ContactModifyEvent -= Modify;
                hooked = false;
            }
        }

        /// <summary>A unit's parts: on the ground (each with its part's mass), or standing (null: gone).</summary>
        public static void Set(IEnumerable<(Collider collider, float mass)> parts, bool? lying)
        {
            foreach (var (collider, mass) in parts)
            {
                if (collider == null)
                {
                    continue;
                }

                Lying.Remove(collider);
                Standing.Remove(collider);
                if (lying == true)
                {
                    Lying[collider] = mass;
                }
                else if (lying == false)
                {
                    Standing.Add(collider);
                }

                // (Only pairs with a modifiable collider come to the callback: the lying parts.)
                collider.hasModifiableContacts = lying == true;
            }

            dirty = true;
        }

        /// <summary>Main thread, before the physics step: the snapshot the physics threads read.</summary>
        public static void Publish()
        {
            if (!dirty)
            {
                return;
            }

            dirty = false;
            var s = new Snapshot();
            foreach (var pair in Lying)
            {
                if (pair.Key != null)
                {
                    s.lying[pair.Key.GetEntityId()] = pair.Value;
                }
            }

            foreach (var c in Standing)
            {
                if (c != null)
                {
                    s.standing.Add(c.GetEntityId());
                }
            }

            current = s;
            if (!hooked && Application.isPlaying)
            {
                Physics.ContactModifyEvent += Modify;
                hooked = true;
            }
        }

        /// <summary>The push a standing part may give a lying part at one contact point (N·s), out of <paramref name="points"/>.</summary>
        public static float Cap(float lyingMass, int points) => lyingMass * MaxNudge / Mathf.Max(1, points);

        private static void Modify(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
        {
            var s = current;
            if (s == null)
            {
                return;
            }

            for (int p = 0; p < pairs.Length; p++)
            {
                var pair = pairs[p];
                EntityId a = pair.colliderEntityId, b = pair.otherColliderEntityId;
                float mass;
                bool capped = (s.lying.TryGetValue(a, out mass) && s.standing.Contains(b))
                              || (s.lying.TryGetValue(b, out mass) && s.standing.Contains(a));
                if (!capped)
                {
                    continue;
                }

                float cap = Cap(mass, pair.contactCount);
                for (int i = 0; i < pair.contactCount; i++)
                {
                    pair.SetMaxImpulse(i, Mathf.Min(pair.GetMaxImpulse(i), cap));
                }
            }
        }
    }
}
