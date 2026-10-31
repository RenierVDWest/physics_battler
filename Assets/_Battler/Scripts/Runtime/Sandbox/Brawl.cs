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
    /// enemy, closes in to its style's engage distance (chest to chest, scaled by size), and when its
    /// timer comes round and it faces the enemy, strikes at a body part one of its moves can reach
    /// (<see cref="MovePlanner"/>). If nothing reaches, it closes in a little more. Used by the
    /// <see cref="BattleSandbox"/> and by the compile harness's dry run.
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
                    Engage(unit, enemy, dt, now);
                }
            }
        }

        private void Engage(UnitController unit, UnitController enemy, float dt, float now)
        {
            var style = unit.Style;
            float size = unit.Body.SizeFactor;
            Vector3 to = enemy.Root.position - unit.Root.position;
            to.y = 0f;
            float distance = to.magnitude;
            float engage = style.engageDistance * size + 0.25f * enemy.Body.SizeFactor;
            if (closeIn.TryGetValue(unit, out float extra))
            {
                engage = Mathf.Max(0.45f * size + 0.2f * enemy.Body.SizeFactor, engage - extra);
            }

            if (distance > engage + 0.05f)
            {
                // Walk in: the root moves, the feet step after it.
                float step = Mathf.Min(distance - engage, style.walkSpeed * size * dt);
                unit.Root.position += to / Mathf.Max(1e-4f, distance) * step;
                return;
            }

            if (!nextAttack.TryGetValue(unit, out float at))
            {
                nextAttack[unit] = now + Next(style) * 0.5f;
                return;
            }

            if (now < at || unit.AngleTo(enemy.Root.position) > 20f || unit.Stance.AnyStepping)
            {
                return;
            }

            var state = MovePlanner.Plan(unit, enemy, random);
            if (state != null)
            {
                unit.StartMove(state);
                closeIn.Remove(unit);
                nextAttack[unit] = now + state.Duration + Next(style);
            }
            else
            {
                closeIn[unit] = (closeIn.TryGetValue(unit, out float e) ? e : 0f) + 0.15f * size;
                nextAttack[unit] = now + 0.4f;
                if (PlaytestTrace.Enabled)
                {
                    PlaytestTrace.Note(unit, "attack", $"nothing reaches {enemy.name} from {distance:0.00} m; closing in");
                }
            }
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
