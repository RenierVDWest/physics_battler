using System.Collections.Generic;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Diagnostics
{
    /// <summary>
    /// Milestone 9c: a held shield is solid, so where it meets another unit's body PhysX pushes the two
    /// apart (its push capped, GroundContacts). This measures how well that holds: each step, each
    /// shield's overlaps with other units' parts (how deep, from the simulation's own poses), followed
    /// from the step one starts to the step it ends. A contact is one shield against one part, from
    /// the first step they overlap to the last.
    /// The targets: under 2 cm deep in 95% of contacts, none lasting longer than 0.2 s.
    /// </summary>
    public sealed class ShieldContacts
    {
        /// <summary>The targets: how deep 95% of contacts stay under, and the longest one (m, s).</summary>
        public const float DepthTarget = 0.02f, LongestTarget = 0.2f;

        /// <summary>One contact, ended.</summary>
        public struct Contact
        {
            public string shield, other;
            public float start, duration, deepest;
        }

        private sealed class Open
        {
            public string shield, other;
            public float start, last, deepest;
            public int seen;
        }

        private readonly Dictionary<(object, object), Open> open = new Dictionary<(object, object), Open>();
        private readonly List<(object, object)> ending = new List<(object, object)>();
        private readonly List<Contact> done = new List<Contact>();
        private int stepNumber;

        /// <summary>The contacts ended so far.</summary>
        public IReadOnlyList<Contact> Done => done;

        /// <summary>A new step: what is not seen in it has ended.</summary>
        public void BeginStep() => stepNumber++;

        /// <summary>Shield <paramref name="shieldId"/> overlaps part <paramref name="otherId"/> by <paramref name="depth"/> at <paramref name="time"/>.</summary>
        public void Seen(object shieldId, object otherId, string shield, string other, float time, float depth)
        {
            var key = (shieldId, otherId);
            if (!open.TryGetValue(key, out var o))
            {
                o = new Open { shield = shield, other = other, start = time, deepest = 0f };
                open[key] = o;
            }

            o.last = time;
            o.deepest = Mathf.Max(o.deepest, depth);
            o.seen = stepNumber;
        }

        /// <summary>Closes the contacts not seen this step (<paramref name="dt"/>: a step's length); returns how many ended.</summary>
        public int EndStep(float dt, List<Contact> ended = null)
        {
            ending.Clear();
            foreach (var pair in open)
            {
                if (pair.Value.seen != stepNumber)
                {
                    ending.Add(pair.Key);
                }
            }

            foreach (var key in ending)
            {
                var c = Close(open[key], dt);
                ended?.Add(c);
                open.Remove(key);
            }

            return ending.Count;
        }

        /// <summary>Closes every contact (the recording stops).</summary>
        public void CloseAll(float dt, List<Contact> ended = null)
        {
            foreach (var o in open.Values)
            {
                var c = Close(o, dt);
                ended?.Add(c);
            }

            open.Clear();
        }

        private Contact Close(Open o, float dt)
        {
            var c = new Contact { shield = o.shield, other = o.other, start = o.start, duration = o.last - o.start + dt, deepest = o.deepest };
            done.Add(c);
            return c;
        }

        /// <summary>The contacts' count, their depth at the 95th percentile and deepest, and the longest (m, s).</summary>
        public static (int count, float p95, float deepest, float longest, float under) Summary(IReadOnlyList<Contact> contacts)
        {
            if (contacts.Count == 0)
            {
                return (0, 0f, 0f, 0f, 1f);
            }

            var depths = new List<float>(contacts.Count);
            float longest = 0f;
            int under = 0;
            foreach (var c in contacts)
            {
                depths.Add(c.deepest);
                longest = Mathf.Max(longest, c.duration);
                under += c.deepest < DepthTarget ? 1 : 0;
            }

            depths.Sort();
            float p95 = depths[Mathf.Clamp(Mathf.CeilToInt(0.95f * depths.Count) - 1, 0, depths.Count - 1)];
            return (contacts.Count, p95, depths[depths.Count - 1], longest, under / (float)contacts.Count);
        }

        /// <summary>
        /// One step's overlaps of <paramref name="unit"/>'s shield with other units' parts, measured
        /// where the simulation has them (<paramref name="label"/> names a unit for the log).
        /// </summary>
        public void Measure(UnitController unit, System.Func<UnitController, string> label, float time)
        {
            var shield = unit != null ? unit.OffWeapon : null;
            if (shield == null || shield.definition == null || !shield.definition.IsShield || !unit.IsPhysical)
            {
                return;
            }

            foreach (var mine in shield.Colliders)
            {
                if (mine == null || mine.isTrigger || !mine.enabled)
                {
                    continue;
                }

                var bounds = mine.bounds;
                PhysicalPose(mine, out Vector3 at, out Quaternion turn);
                foreach (var other in Physics.OverlapSphere(at + (bounds.center - mine.transform.position), 0.5f * bounds.size.magnitude + 0.05f, ~0, QueryTriggerInteraction.Ignore))
                {
                    var part = other != null ? other.GetComponentInParent<BodyPart>() : null;
                    if (part == null || part.Owner == null || part.Owner == unit || part.partCollider != other)
                    {
                        continue;
                    }

                    PhysicalPose(other, out Vector3 otherAt, out Quaternion otherTurn);
                    if (Physics.ComputePenetration(mine, at, turn, other, otherAt, otherTurn, out _, out float depth) && depth > 1e-4f)
                    {
                        Seen(mine, other, label(unit) + "'s shield", label(part.Owner) + " " + part.id, time, depth);
                    }
                }
            }
        }

        /// <summary>Where the simulation has a collider (inside FixedUpdate an interpolated body's transform trails it).</summary>
        private static void PhysicalPose(Collider collider, out Vector3 position, out Quaternion rotation)
        {
            var t = collider.transform;
            var body = collider.attachedRigidbody;
            if (body == null)
            {
                position = t.position;
                rotation = t.rotation;
                return;
            }

            var bt = body.transform;
            Quaternion toBody = Quaternion.Inverse(bt.rotation);
            position = body.position + body.rotation * (toBody * (t.position - bt.position));
            rotation = body.rotation * (toBody * t.rotation);
        }
    }
}
