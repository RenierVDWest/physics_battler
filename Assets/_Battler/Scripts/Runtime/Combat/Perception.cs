using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>How well a unit perceives something now: not at all, only by touch, at the edge of its view, or in clear view.</summary>
    public enum Sight
    {
        Unseen,
        Felt,
        Edge,
        Clear,
    }

    /// <summary>
    /// How a unit sees (milestone 9): its field of view from where its head points, how much of it
    /// is clear, how much slower it reacts to what it only catches at the edge, how long it
    /// remembers what it no longer sees, and how close something must be to be felt. Per unit:
    /// one is wide-eyed, another tunnel-visioned.
    /// </summary>
    [Serializable]
    public struct PerceptionSkill
    {
        [Tooltip("How far round from where the head points it sees anything at all (degrees each way, across). People: about 100.")]
        public float fieldOfView;
        [Tooltip("How far up and down it sees (degrees each way).")]
        public float fieldUpDown;
        [Tooltip("Within this angle of where the head points it sees clearly: a blow's line, an opening (degrees).")]
        public float clearView;
        [Tooltip("How much slower it reacts to what it catches only at the edge of its view (times its reaction time).")]
        public float edgeReaction;
        [Tooltip("How long it remembers a unit it no longer sees (seconds).")]
        public float memory;
        [Tooltip("Within this distance anything is felt, if not seen (metres at medium size).")]
        public float feltRadius;

        public static PerceptionSkill Default => new PerceptionSkill
        {
            fieldOfView = 100f, fieldUpDown = 70f, clearView = 30f, edgeReaction = 1.5f, memory = 3f, feltRadius = 0.9f,
        };

        /// <summary>This skill with anything unset taken from the default (an asset saved before milestone 9).</summary>
        public PerceptionSkill Valid()
        {
            var d = Default;
            var s = this;
            if (s.fieldOfView <= 0f) s.fieldOfView = d.fieldOfView;
            if (s.fieldUpDown <= 0f) s.fieldUpDown = d.fieldUpDown;
            if (s.clearView <= 0f) s.clearView = d.clearView;
            if (s.edgeReaction <= 0f) s.edgeReaction = d.edgeReaction;
            if (s.memory <= 0f) s.memory = d.memory;
            if (s.feltRadius <= 0f) s.feltRadius = d.feltRadius;
            s.clearView = Mathf.Min(s.clearView, s.fieldOfView);
            return s;
        }
    }

    /// <summary>What a head sees: a point in clear view, at the edge of its view, felt close by, or not at all.</summary>
    public static class Perception
    {
        /// <param name="eye">Where it sees from (between the eyes).</param>
        /// <param name="head">Which way the head points (its forward).</param>
        /// <param name="felt">How close something is felt (metres, already scaled).</param>
        public static Sight Classify(Vector3 eye, Quaternion head, Vector3 point, float felt, in PerceptionSkill skill)
        {
            Vector3 to = point - eye;
            if (to.sqrMagnitude < 1e-8f)
            {
                return Sight.Clear;
            }

            Vector3 local = Quaternion.Inverse(head) * to;
            float angle = Vector3.Angle(Vector3.forward, local);
            if (angle <= skill.clearView)
            {
                return Sight.Clear;
            }

            float across = Mathf.Abs(Mathf.Atan2(local.x, local.z)) * Mathf.Rad2Deg;
            float upDown = Mathf.Abs(Mathf.Atan2(local.y, new Vector2(local.x, local.z).magnitude)) * Mathf.Rad2Deg;
            if (across <= skill.fieldOfView && upDown <= skill.fieldUpDown)
            {
                return Sight.Edge;
            }

            return to.magnitude <= felt ? Sight.Felt : Sight.Unseen;
        }

        /// <summary>Its reaction time against a blow seen this way (times its own; infinite: it can't react before contact).</summary>
        public static float ReactionScale(Sight sight, in PerceptionSkill skill)
        {
            switch (sight)
            {
                case Sight.Clear: return 1f;
                case Sight.Edge: return skill.edgeReaction;
                default: return float.PositiveInfinity;
            }
        }

        /// <summary>Can it react to a blow it perceives this way? (Only what it sees: a touch comes too late.)</summary>
        public static bool CanReact(Sight sight) => sight >= Sight.Edge;
    }

    /// <summary>One thing a unit knows about: where it was last perceived, how it was moving, when, and how clearly.</summary>
    public struct Known
    {
        public Vector3 position;
        public Vector3 velocity;
        public float seenAt;
        public float firstSeenAt;
        public Sight sight;
    }

    /// <summary>
    /// What a unit knows of the others (milestone 9): each one it has perceived, where and when it
    /// was last perceived and how it was moving. One out of view is remembered for the unit's
    /// memory span, its position carried along its last motion, then forgotten.
    /// </summary>
    public sealed class Knowledge<T> where T : class
    {
        private readonly Dictionary<T, Known> known = new Dictionary<T, Known>();
        private readonly List<T> scratch = new List<T>();

        public int Count => known.Count;
        public IEnumerable<KeyValuePair<T, Known>> All => known;

        /// <summary>It perceived <paramref name="who"/> now (nothing happens for <see cref="Sight.Unseen"/>). True the first time.</summary>
        public bool Observe(T who, Vector3 position, Sight sight, float now)
        {
            if (sight == Sight.Unseen || who == null)
            {
                return false;
            }

            if (known.TryGetValue(who, out var k))
            {
                float dt = now - k.seenAt;
                if (dt > 1e-4f)
                {
                    Vector3 v = (position - k.position) / dt;
                    v.y = 0f;
                    k.velocity = Vector3.Lerp(k.velocity, v, dt > 0.25f ? 1f : 0.3f);
                }

                k.position = position;
                k.seenAt = now;
                k.sight = sight;
                known[who] = k;
                return false;
            }

            known[who] = new Known { position = position, seenAt = now, firstSeenAt = now, sight = sight };
            return true;
        }

        public bool Knows(T who) => who != null && known.ContainsKey(who);

        public bool TryGet(T who, out Known k) => known.TryGetValue(who, out k);

        /// <summary>When it last looked about (<see cref="BeginLook"/>): what it perceived then is what it perceives now.</summary>
        public float LastLook { get; private set; } = float.NegativeInfinity;

        /// <summary>A new look about at <paramref name="now"/>: everything it perceives is observed after this.</summary>
        public void BeginLook(float now) => LastLook = now;

        /// <summary>How it perceives <paramref name="who"/> now: as at its last look about (unseen if it wasn't perceived then).</summary>
        public Sight Current(T who) => who != null && known.TryGetValue(who, out var k) && k.seenAt >= LastLook - 1e-5f ? k.sight : Sight.Unseen;

        /// <summary>Where it thinks <paramref name="who"/> is: last seen, carried along its motion (at most a second's worth).</summary>
        public Vector3 Estimate(T who, float now)
        {
            var k = known[who];
            return k.position + k.velocity * Mathf.Clamp(now - k.seenAt, 0f, 1f);
        }

        /// <summary>Forgets what it hasn't perceived for longer than its memory, and anything <paramref name="gone"/> says is gone. What it forgot.</summary>
        public List<T> Forget(float now, float memory, Func<T, bool> gone = null)
        {
            scratch.Clear();
            foreach (var pair in known)
            {
                if (now - pair.Value.seenAt > memory || (gone != null && gone(pair.Key)))
                {
                    scratch.Add(pair.Key);
                }
            }

            foreach (var who in scratch)
            {
                known.Remove(who);
            }

            return scratch;
        }

        public void Clear() => known.Clear();
    }
}
