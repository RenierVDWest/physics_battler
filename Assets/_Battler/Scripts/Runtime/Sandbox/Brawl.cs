using System.Collections.Generic;
using PhysicsBattler.Combat;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Motion;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>
    /// Sandbox gameplay until milestone 6 brings the real fight loop: every unit faces its nearest
    /// enemy and closes in to its style's engage distance (chest to chest, scaled by size). When its
    /// timer comes round it strikes at any enemy in reach in front of it, at a body part one of its
    /// moves can land on cleanly (<see cref="MovePlanner"/>). If nothing lands, it closes in a little
    /// when too far, or gives ground when too close. Used by the <see cref="BattleSandbox"/> and by
    /// the compile harness's dry run.
    /// </summary>
    public sealed class Brawl
    {
        private readonly Dictionary<UnitController, float> nextAttack = new Dictionary<UnitController, float>();
        private readonly Dictionary<UnitController, float> closeIn = new Dictionary<UnitController, float>();
        private readonly System.Random random;

        /// <summary>Units close in and strike; off, they only face each other.</summary>
        public bool Fight = true;

        /// <summary>Everyone faces away from the enemy (to test turning and stepping).</summary>
        public bool TurnedAway;

        public Brawl(int seed)
        {
            random = seed != 0 ? new System.Random(seed) : new System.Random();
        }

        public void Step(IReadOnlyList<UnitController> units, float dt, float now)
        {
            foreach (var unit in units)
            {
                if (unit == null || !unit.IsInitialized)
                {
                    continue;
                }

                var enemy = Nearest(units, unit);
                if (enemy == null)
                {
                    continue;
                }

                if (TurnedAway)
                {
                    unit.FacePoint(unit.Root.position * 2f - enemy.Root.position);
                    continue;
                }

                unit.FacePoint(enemy.Root.position);
                if (Fight && !unit.IsAttacking)
                {
                    Engage(units, unit, enemy, dt, now);
                }
            }
        }

        private void Engage(IReadOnlyList<UnitController> units, UnitController unit, UnitController enemy, float dt, float now)
        {
            var style = unit.Style;
            float size = unit.Body.SizeFactor;
            Vector3 to = enemy.Root.position - unit.Root.position;
            to.y = 0f;
            float distance = to.magnitude;
            float engage = EngageDistance(unit, enemy);
            float adjust = closeIn.TryGetValue(unit, out float a) ? a : 0f;
            engage = Mathf.Max(0.45f * size + 0.2f * enemy.Body.SizeFactor, engage - adjust);

            if (distance > engage + 0.05f)
            {
                // Walk in: the root moves, the feet step after it.
                float step = Mathf.Min(distance - engage, style.walkSpeed * size * dt);
                unit.Root.position += to / Mathf.Max(1e-4f, distance) * step;
                return;
            }

            if (adjust < 0f && distance < engage - 0.05f)
            {
                // Too close to strike without hitting itself: back off a little.
                float step = Mathf.Min(engage - distance, 0.6f * style.walkSpeed * size * dt);
                unit.Root.position -= to / Mathf.Max(1e-4f, distance) * step;
                return;
            }

            if (!nextAttack.TryGetValue(unit, out float at))
            {
                nextAttack[unit] = now + Next(style) * 0.5f;
                return;
            }

            if (now < at || unit.Stance.AnyStepping)
            {
                return;
            }

            // Strike any enemy in reach in front of it, not only the nearest one.
            var candidates = new List<UnitController>();
            foreach (var other in units)
            {
                if (other == null || other == unit || other.Team == unit.Team || !other.IsInitialized)
                {
                    continue;
                }

                Vector3 d = other.Root.position - unit.Root.position;
                d.y = 0f;
                if (d.magnitude <= EngageDistance(unit, other) + 0.5f * size && unit.AngleTo(other.Root.position) <= 35f)
                {
                    candidates.Add(other);
                }
            }

            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            foreach (var target in candidates)
            {
                var state = MovePlanner.Plan(unit, target, random);
                if (state != null)
                {
                    unit.StartMove(state);
                    closeIn.Remove(unit);
                    nextAttack[unit] = now + state.Duration + Next(style);
                    return;
                }
            }

            // Nothing lands cleanly: too far, close in; too close (the blow would pass through its own
            // body, or needs more room than a step back), give ground.
            bool tooClose = distance < EngageDistance(unit, enemy) - 0.1f * size;
            float change = (tooClose ? -0.15f : 0.15f) * size;
            closeIn[unit] = Mathf.Clamp(adjust + change, -0.5f * size, 0.9f * size);
            nextAttack[unit] = now + 0.4f;
            if (PlaytestTrace.Enabled)
            {
                PlaytestTrace.Note(unit, "attack", $"nothing lands on {enemy.name} from {distance:0.00} m; {(tooClose ? "backing off" : "closing in")}");
            }
        }

        /// <summary>Chest to chest at medium size (the style's), scaled by both units' sizes.</summary>
        private static float EngageDistance(UnitController unit, UnitController enemy)
        {
            return unit.Style.engageDistance * unit.Body.SizeFactor + 0.25f * enemy.Body.SizeFactor;
        }

        private float Next(CombatStyle style)
        {
            return Mathf.Lerp(style.attackInterval.x, style.attackInterval.y, (float)random.NextDouble());
        }

        private static UnitController Nearest(IReadOnlyList<UnitController> units, UnitController unit)
        {
            UnitController best = null;
            float bestDistance = float.MaxValue;
            foreach (var other in units)
            {
                if (other == null || other == unit || other.Team == unit.Team || !other.IsInitialized)
                {
                    continue;
                }

                float d = (other.Root.position - unit.Root.position).sqrMagnitude;
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = other;
                }
            }

            return best;
        }
    }
}
