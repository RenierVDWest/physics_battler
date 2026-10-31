using System.Collections.Generic;
using PhysicsBattler.Board;
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

        /// <summary>The board: a unit that got up goes back onto the nearest free tile before it fights on (null: it fights on where it stands).</summary>
        public BoardGrid Grid;

        // The tile each unit getting back onto the board is heading for (so two don't pick the same one).
        private readonly Dictionary<UnitController, Tile> spots = new Dictionary<UnitController, Tile>();
        private readonly Dictionary<UnitController, float> spotSince = new Dictionary<UnitController, float>();

        // Units whose lead-in (a feint before the blow) has played, and when it ended: their next
        // attack goes ahead without another (for a moment: a blow that never comes leads in afresh).
        private readonly Dictionary<UnitController, float> ledIn = new Dictionary<UnitController, float>();

        public Brawl(int seed)
        {
            random = seed != 0 ? new System.Random(seed) : new System.Random();
        }

        public void Step(IReadOnlyList<UnitController> units, float dt, float now)
        {
            foreach (var unit in units)
            {
                if (unit == null || !unit.IsInitialized || !unit.IsAlive)
                {
                    spots.Remove(unit);
                    continue;
                }

                // Off its feet (stumbling, down, getting up): nothing to decide until it stands.
                if (!unit.CanAct)
                {
                    ledIn.Remove(unit);   // (the next blow after it gets up leads in afresh)
                    continue;
                }

                if (!unit.NeedsSpot)
                {
                    spots.Remove(unit);   // (moved by the sandbox, or there already)
                    spotSince.Remove(unit);
                }
                else if (BackOntoTile(units, unit, dt, now))
                {
                    continue;
                }

                var enemy = Nearest(units, unit);
                unit.Opponent = TurnedAway ? null : enemy;
                unit.CircleOpponent = Fight;   // (just facing each other, they stay where they are)
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
                if (Fight && !unit.IsAttacking && unit.Knockback.magnitude < 0.05f && unit.HitStop <= 0f && !enemy.IsDown)
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

            // Its Fight Rhythm may lead into the blow first (a boxer's feint before the real swing):
            // the blow comes as the lead-in finishes.
            // (It waits for the whole lead-in: a feint's wind-up is back at the guard by then, so the
            // real blow winds up from there rather than snapping back first.)
            if (!ledIn.TryGetValue(unit, out float ledAt) || now - ledAt > 1.5f)
            {
                float lead = unit.LeadIn();
                if (lead > 0f)
                {
                    ledIn[unit] = now + lead;
                    nextAttack[unit] = now + lead;
                    return;
                }
            }

            // Strike any enemy in reach in front of it, not only the nearest one.
            var candidates = new List<UnitController>();
            foreach (var other in units)
            {
                if (other == null || other == unit || other.Team == unit.Team || !other.IsInitialized || !other.IsAlive || other.IsDown)
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
                    if (unit.StartMove(state))
                    {
                        ledIn.Remove(unit);
                    }

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

        /// <summary>
        /// Walks a unit that just got up to the middle of the nearest free tile (no other unit on it
        /// or heading for it). True while it is on its way.
        /// </summary>
        private bool BackOntoTile(IReadOnlyList<UnitController> units, UnitController unit, float dt, float now)
        {
            if (Grid == null)
            {
                unit.NeedsSpot = false;
                return false;
            }

            // Someone else stood on the tile meanwhile: pick another. Never more than a few seconds.
            if (spots.TryGetValue(unit, out var held) && (Occupied(units, unit, held) || now - spotSince[unit] > 6f))
            {
                spots.Remove(unit);
                if (now - spotSince[unit] > 6f)
                {
                    unit.NeedsSpot = false;
                    spotSince.Remove(unit);
                    return false;
                }
            }

            if (!spots.TryGetValue(unit, out var tile))
            {
                if (!NearestFreeTile(units, unit, out tile))
                {
                    unit.NeedsSpot = false;
                    spotSince.Remove(unit);
                    return false;
                }

                spots[unit] = tile;
                if (!spotSince.ContainsKey(unit))
                {
                    spotSince[unit] = now;
                }

                if (PlaytestTrace.Enabled)
                {
                    PlaytestTrace.Note(unit, "move", $"back onto the board: heading for tile {tile}");
                }
            }

            Vector3 to = Grid.Center(tile) - unit.Root.position;
            to.y = 0f;
            float distance = to.magnitude;
            if (distance < 0.05f * unit.Body.SizeFactor)
            {
                unit.NeedsSpot = false;
                spots.Remove(unit);
                spotSince.Remove(unit);
                return false;
            }

            unit.FacePoint(Grid.Center(tile));
            float step = Mathf.Min(distance, unit.Style.walkSpeed * unit.Body.SizeFactor * dt);
            unit.Root.position += to / distance * step;
            return true;
        }

        /// <summary>Another living unit stands on the tile (within half a tile of its middle).</summary>
        private bool Occupied(IReadOnlyList<UnitController> units, UnitController unit, Tile tile)
        {
            Vector3 center = Grid.Center(tile);
            foreach (var other in units)
            {
                if (other == null || other == unit || !other.IsInitialized || !other.IsAlive)
                {
                    continue;
                }

                Vector3 d = other.Root.position - center;
                d.y = 0f;
                if (d.magnitude < 0.5f * Grid.tileSize)
                {
                    return true;
                }
            }

            return false;
        }

        private bool NearestFreeTile(IReadOnlyList<UnitController> units, UnitController unit, out Tile best)
        {
            best = default;
            float bestDistance = float.MaxValue;
            float clear = 0.5f * Grid.tileSize;
            for (int c = 0; c < Grid.columns; c++)
            {
                for (int r = 0; r < Grid.rows; r++)
                {
                    var tile = new Tile(c, r);
                    Vector3 center = Grid.Center(tile);
                    bool taken = false;
                    foreach (var pair in spots)
                    {
                        taken |= pair.Key != unit && pair.Value.column == c && pair.Value.row == r;
                    }

                    foreach (var other in units)
                    {
                        if (taken)
                        {
                            break;
                        }

                        if (other == null || other == unit || !other.IsInitialized || !other.IsAlive)
                        {
                            continue;
                        }

                        Vector3 d = other.Root.position - center;
                        d.y = 0f;
                        taken = d.magnitude < clear;
                    }

                    if (taken)
                    {
                        continue;
                    }

                    Vector3 to = center - unit.Root.position;
                    to.y = 0f;
                    if (to.sqrMagnitude < bestDistance)
                    {
                        bestDistance = to.sqrMagnitude;
                        best = tile;
                    }
                }
            }

            return bestDistance < float.MaxValue;
        }

        private static UnitController Nearest(IReadOnlyList<UnitController> units, UnitController unit)
        {
            UnitController best = null;
            float bestDistance = float.MaxValue;
            foreach (var other in units)
            {
                if (other == null || other == unit || other.Team == unit.Team || !other.IsInitialized || !other.IsAlive)
                {
                    continue;
                }

                // Someone on the ground is no one to fight while there is anyone standing.
                float d = (other.Root.position - unit.Root.position).sqrMagnitude * (other.IsDown ? 100f : 1f);
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
