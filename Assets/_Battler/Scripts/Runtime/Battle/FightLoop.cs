using System;
using System.Collections.Generic;
using PhysicsBattler.Board;
using PhysicsBattler.Combat;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Battle
{
    /// <summary>
    /// Milestone 6: the fight loop. Two teams on the board fight it out with no help:
    /// <list type="bullet">
    /// <item>Each unit picks a target: the nearest enemy that can still fight, by the walk there,
    /// keeping the one it has, and favouring one that is already hurt or already fighting it.</item>
    /// <item>It walks tile to tile (the root follows the path between tile centres; its captured
    /// walk plays) to a free tile next to the target, the tiles it stands on and is heading for held
    /// so no two units share one. A body lying on the board holds the tile it lies on.</item>
    /// <item>On its tile it closes in or gives ground a little (never off the tile), and when its
    /// timer comes round it strikes any enemy in reach in front of it at a body part a move can land
    /// on. If it reads an opening in the target (a posture's window, a tired guard: <see
    /// cref="UnitController.Openings"/>), it aims at the open part, and goes early to catch it.</item>
    /// <item>The fight is over when one team has no one left who can fight: dead, unarmed or
    /// legless (a time limit ends it as a draw on what is left).</item>
    /// </list>
    /// </summary>
    public sealed class FightLoop
    {
        public BoardGrid Grid = new BoardGrid();

        /// <summary>Off: units stand on their tiles and only face each other.</summary>
        public bool Fight = true;

        /// <summary>Seconds of fighting before it ends as a draw on what is left.</summary>
        public float TimeLimit = 240f;

        public bool Over { get; private set; }

        /// <summary>The team that won, or -1 (a draw, or not over).</summary>
        public int Winner { get; private set; } = -1;

        /// <summary>Why it ended (for the log and the screen).</summary>
        public string Result { get; private set; } = "";

        public float StartedAt { get; private set; } = -1f;
        public float EndedAt { get; private set; } = -1f;

        /// <summary>The fight ended (this loop, the winning team or -1, why).</summary>
        public static event Action<FightLoop, int, string> Ended;

        private sealed class Plan
        {
            public bool placed;
            public Tile tile;
            public readonly List<Tile> path = new List<Tile>();
            public bool hasDest;
            public Tile dest;
            public UnitController target;
            public float retargetAt;
            public float nextAttack = -1f;
            public bool readsThisTime;
            public float ledInAt = -999f;
            public float closeIn;
            public float progressAt;
            public Vector3 progressFrom;
            public float routeAt = -999f;
        }

        private readonly Dictionary<UnitController, Plan> plans = new Dictionary<UnitController, Plan>();
        private readonly System.Random random;

        public FightLoop(int seed)
        {
            random = seed != 0 ? new System.Random(seed) : new System.Random();
        }

        /// <summary>The unit can still fight: alive, armed and on its feet's worth of legs.</summary>
        public static bool CanFight(UnitController unit)
        {
            return unit != null && unit.IsInitialized && unit.IsAlive && !unit.LegLost && unit.Loadout?.Main != null;
        }

        /// <summary>The tile a unit stands on (or is walking to), for others to keep off.</summary>
        public bool TileOf(UnitController unit, out Tile tile)
        {
            if (plans.TryGetValue(unit, out var plan) && plan.placed)
            {
                tile = plan.tile;
                return true;
            }

            tile = default;
            return false;
        }

        public void Step(IReadOnlyList<UnitController> units, float dt, float now)
        {
            if (StartedAt < 0f)
            {
                StartedAt = now;
            }

            var taken = Occupancy(units);
            if (Fight && !Over)
            {
                CheckOver(units, now);
            }

            foreach (var unit in units)
            {
                if (unit == null || !unit.IsInitialized)
                {
                    continue;
                }

                var plan = PlanOf(unit);
                if (!unit.IsAlive || !unit.CanAct)
                {
                    // Off its feet or gone: no path, nothing to do; on the ground, where it lies is its tile.
                    plan.path.Clear();
                    plan.hasDest = false;
                    if (!unit.IsAlive || unit.IsDown || !plan.placed)
                    {
                        plan.tile = Clamp(Grid.TileAt(unit.Rig.Part(PartId.Pelvis).transform.position));
                        plan.placed = true;
                    }

                    continue;
                }

                if (!plan.placed)
                {
                    plan.tile = Clamp(Grid.TileAt(unit.Root.position));
                    plan.placed = true;
                    plan.progressAt = now;
                    plan.progressFrom = unit.Root.position;
                }

                if (!Fight)
                {
                    // (Not fighting: it stays where it was put, facing the nearest enemy; it finds its
                    // tile again when the fight resumes.)
                    var near = Nearest(units, unit, false);
                    unit.Opponent = near;
                    unit.CircleOpponent = false;
                    if (near != null)
                    {
                        unit.FacePoint(near.Root.position);
                    }

                    plan.path.Clear();
                    plan.hasDest = false;
                    plan.placed = false;
                    continue;
                }

                // Back onto the board after getting up, or knocked off its tile, or onto someone
                // else's: to the nearest free tile (its own, if it is still free).
                bool settled = OnTile(unit, plan.tile) && (!taken.TryGetValue(Key(plan.tile), out var holder) || holder == unit);
                if (plan.path.Count == 0 && (unit.NeedsSpot || !settled))
                {
                    if (!settled)
                    {
                        // (Its own tile, if no one else holds it: just step back to its middle.)
                        if (taken.TryGetValue(Key(plan.tile), out var on) && on != unit)
                        {
                            plan.tile = Clamp(Grid.TileAt(unit.Root.position));
                        }

                        if (taken.TryGetValue(Key(plan.tile), out on) && on != unit && NearestFree(taken, unit, out var free))
                        {
                            Route(plan, unit, free, taken, now, "onto a free tile");
                        }
                    }

                    if (plan.path.Count > 0)
                    {
                        Walk(unit, plan, null, taken, dt, now);
                        continue;
                    }

                    if (!OnTile(unit, plan.tile))
                    {
                        WalkInside(unit, Grid.Center(plan.tile), dt);
                        continue;
                    }

                    unit.NeedsSpot = false;
                }

                if (Over || !CanFight(unit))
                {
                    // (Out of it, or it's over: it stands on its tile facing the nearest enemy, if any.)
                    var near = Nearest(units, unit, false);
                    unit.Opponent = null;
                    unit.CircleOpponent = false;
                    if (near != null)
                    {
                        unit.FacePoint(near.Root.position);
                    }

                    plan.path.Clear();
                    plan.hasDest = false;
                    continue;
                }

                unit.CircleOpponent = true;
                var target = ChooseTarget(units, unit, plan, now);
                unit.Opponent = target;
                if (target == null)
                {
                    continue;
                }

                if (plan.path.Count > 0)
                {
                    Walk(unit, plan, target, taken, dt, now);
                    continue;
                }

                Vector3 to = target.Root.position - unit.Root.position;
                to.y = 0f;
                float distance = to.magnitude;
                float reach = EngageDistance(unit, target) + 0.5f * unit.Body.SizeFactor;
                if (distance > reach + 0.15f * Grid.tileSize || !Adjacent(plan.tile, TargetTile(target)))
                {
                    // Not close enough from this tile: walk to a free tile next to it.
                    if (now - plan.routeAt > 0.4f && BestTile(unit, target, taken, out var dest))
                    {
                        if (!SameTile(dest, plan.tile))
                        {
                            Route(plan, unit, dest, taken, now, $"toward {target.name}");
                            if (plan.path.Count > 0)
                            {
                                continue;
                            }
                        }
                    }
                }

                unit.FacePoint(target.Root.position);
                if (unit.HitStop <= 0f && !unit.IsAttacking && unit.Knockback.magnitude < 0.05f)
                {
                    Engage(units, unit, target, plan, dt, now);
                }
            }
        }

        // ---- The end ---------------------------------------------------------------------------

        private void CheckOver(IReadOnlyList<UnitController> units, float now)
        {
            var standing = new int[2];
            var health = new float[2];
            bool any0 = false, any1 = false;
            foreach (var u in units)
            {
                if (u == null || !u.IsInitialized || u.Team < 0 || u.Team > 1)
                {
                    continue;
                }

                any0 |= u.Team == 0;
                any1 |= u.Team == 1;
                if (CanFight(u))
                {
                    standing[u.Team]++;
                    health[u.Team] += u.Health / Mathf.Max(1f, u.Profile.maxHealth);
                }
            }

            if (!any0 || !any1)
            {
                return;   // (one team on the board: nothing to fight)
            }

            if (standing[0] == 0 || standing[1] == 0)
            {
                Finish(standing[0] > 0 ? 0 : standing[1] > 0 ? 1 : -1, standing[0] + standing[1] == 0 ? "no one left who can fight" : "the other team can't fight", now);
            }
            else if (now - StartedAt > TimeLimit)
            {
                int winner = standing[0] != standing[1] ? (standing[0] > standing[1] ? 0 : 1)
                    : Mathf.Abs(health[0] - health[1]) > 0.05f ? (health[0] > health[1] ? 0 : 1) : -1;
                Finish(winner, $"time ({TimeLimit:0} s): {standing[0]} against {standing[1]} still fighting", now);
            }
        }

        private void Finish(int winner, string why, float now)
        {
            Over = true;
            Winner = winner;
            EndedAt = now;
            Result = (winner < 0 ? "a draw" : $"team {(winner == 0 ? "A" : "B")} wins") + $" after {now - StartedAt:0} s ({why})";
            if (PlaytestTrace.Enabled)
            {
                PlaytestTrace.Note(null, "fight", "over: " + Result);
            }

            Ended?.Invoke(this, winner, Result);
        }

        // ---- Choosing ---------------------------------------------------------------------------

        private UnitController ChooseTarget(IReadOnlyList<UnitController> units, UnitController unit, Plan plan, float now)
        {
            var current = plan.target;
            bool valid = current != null && current.IsInitialized && current.IsAlive && current.Team != unit.Team && !current.IsDown;
            if (valid && now < plan.retargetAt)
            {
                return current;
            }

            plan.retargetAt = now + 0.5f;
            UnitController best = null;
            float bestScore = float.MaxValue;
            foreach (var other in units)
            {
                if (other == null || !other.IsInitialized || !other.IsAlive || other.Team == unit.Team)
                {
                    continue;
                }

                // How far (in tiles), less for one already hurt, fighting it, or its target already;
                // one on the ground only when no one is standing.
                float score = Vector3.Distance(other.Root.position, unit.Root.position) / Grid.tileSize;
                score -= 0.8f * (1f - other.Health / Mathf.Max(1f, other.Profile.maxHealth));
                if (other.Opponent == unit)
                {
                    score -= 0.7f;
                }

                if (other == current)
                {
                    score -= 0.9f;   // (it keeps its target unless another is clearly better)
                }

                if (other.IsDown || !CanFight(other))
                {
                    score += 50f;
                }

                if (score < bestScore)
                {
                    bestScore = score;
                    best = other;
                }
            }

            if (best != null && best.IsDown)
            {
                best = null;   // (only units on the ground left: it waits for one to get up)
            }

            if (best != current && best != null && PlaytestTrace.Enabled)
            {
                PlaytestTrace.Note(unit, "target", $"{best.name} ({Vector3.Distance(best.Root.position, unit.Root.position):0.0} m, health {best.Health:0})");
            }

            if (best != current)
            {
                plan.closeIn = 0f;
            }

            plan.target = best;
            return best;
        }

        // ---- Fighting on its tile -----------------------------------------------------------------

        private void Engage(IReadOnlyList<UnitController> units, UnitController unit, UnitController enemy, Plan plan, float dt, float now)
        {
            var style = unit.Style;
            float size = unit.Body.SizeFactor;
            Vector3 to = enemy.Root.position - unit.Root.position;
            to.y = 0f;
            float distance = to.magnitude;
            float engage = Mathf.Max(0.45f * size + 0.2f * enemy.Body.SizeFactor, EngageDistance(unit, enemy) - plan.closeIn);
            Vector3 toward = to / Mathf.Max(1e-4f, distance);

            // Closing in or giving ground a little, never off its tile.
            if (distance > engage + 0.05f)
            {
                WalkInside(unit, unit.Root.position + toward * Mathf.Min(distance - engage, unit.WalkSpeed * dt), dt, plan.tile);
                return;
            }

            if (plan.closeIn < 0f && distance < engage - 0.05f)
            {
                WalkInside(unit, unit.Root.position - toward * Mathf.Min(engage - distance, 0.6f * unit.WalkSpeed * dt), dt, plan.tile);
                return;
            }

            if (plan.nextAttack < 0f)
            {
                plan.nextAttack = now + Next(style) * 0.5f;
                plan.readsThisTime = random.NextDouble() < style.readsOpenings;
                return;
            }

            // An opening it reads: strike into it now if the blow was coming soon anyway.
            var open = enemy.Openings;
            bool opening = plan.readsThisTime && open != AimParts.None && now >= plan.nextAttack - style.openingLead;
            if ((now < plan.nextAttack && !opening) || unit.Stance.AnyStepping)
            {
                return;
            }

            // Its Fight Rhythm may lead into the blow first (not into an opening: that would waste it).
            if (!opening && now - plan.ledInAt > 1.5f)
            {
                float lead = unit.LeadIn();
                if (lead > 0f)
                {
                    plan.ledInAt = now + lead;
                    plan.nextAttack = now + lead;
                    return;
                }
            }

            // Its target first, then any other enemy in reach in front of it.
            var candidates = new List<UnitController> { enemy };
            foreach (var other in units)
            {
                if (other == null || other == unit || other == enemy || other.Team == unit.Team || !other.IsInitialized || !other.IsAlive || other.IsDown)
                {
                    continue;
                }

                Vector3 d = other.Root.position - unit.Root.position;
                d.y = 0f;
                if (d.magnitude <= EngageDistance(unit, other) + 0.5f * size && unit.AngleTo(other.Root.position) <= 35f)
                {
                    candidates.Insert(1 + random.Next(candidates.Count), other);
                }
            }

            foreach (var target in candidates)
            {
                var prefer = target == enemy && plan.readsThisTime ? target.Openings : AimParts.None;
                var state = MovePlanner.Plan(unit, target, random, prefer, 4f);
                if (state == null)
                {
                    continue;
                }

                if (unit.StartMove(state))
                {
                    plan.ledInAt = -999f;
                    plan.closeIn = 0f;
                    if (PlaytestTrace.Enabled && prefer != AimParts.None)
                    {
                        PlaytestTrace.Note(unit, "attack", $"read an opening in {target.name} ({prefer.ToString().Replace(", ", " ")}): {state.Move.name.Replace("MD_", "")} at the {state.Part}"
                                                           + (prefer.Contains(state.Part) ? "" : " (the open part couldn't be reached)"));
                    }
                }

                plan.nextAttack = now + state.Duration + Next(style);
                plan.readsThisTime = random.NextDouble() < style.readsOpenings;
                return;
            }

            // Nothing lands cleanly: too far, close in; too close, give ground.
            bool tooClose = distance < EngageDistance(unit, enemy) - 0.1f * size;
            plan.closeIn = Mathf.Clamp(plan.closeIn + (tooClose ? -0.15f : 0.15f) * size, -0.5f * size, 0.9f * size);
            plan.nextAttack = now + 0.4f;
            if (PlaytestTrace.Enabled)
            {
                PlaytestTrace.Note(unit, "attack", $"nothing lands on {enemy.name} from {distance:0.00} m; {(tooClose ? "backing off" : "closing in")}");
            }
        }

        // ---- Walking the board ---------------------------------------------------------------------

        /// <summary>Along its path: the root walks to each tile centre in turn (facing the way, or its target once close).</summary>
        private void Walk(UnitController unit, Plan plan, UnitController target, Dictionary<long, UnitController> taken, float dt, float now)
        {
            var next = plan.path[0];
            // Someone else stepped onto it meanwhile: find another way.
            if (taken.TryGetValue(Key(next), out var other) && other != unit && other != null)
            {
                plan.path.Clear();
                plan.hasDest = false;
                plan.routeAt = now - 1f;
                return;
            }

            Vector3 center = Grid.Center(next);
            Vector3 to = center - unit.Root.position;
            to.y = 0f;
            float speed = unit.WalkSpeed;
            float step = speed * dt;
            if (target != null && Vector3.Distance(target.Root.position, unit.Root.position) < 2.6f * Grid.tileSize)
            {
                unit.FacePoint(target.Root.position);
            }
            else if (to.sqrMagnitude > 1e-4f)
            {
                unit.FacePoint(unit.Root.position + to);
            }

            // It holds the tile it stands on until it is closer to the next.
            if (Vector3.Distance(unit.Root.position, center) < 0.5f * Grid.tileSize)
            {
                plan.tile = next;
            }

            if (to.magnitude <= step)
            {
                unit.Root.position = new Vector3(center.x, unit.Root.position.y, center.z);
                plan.tile = next;
                plan.path.RemoveAt(0);
                if (plan.path.Count == 0)
                {
                    plan.hasDest = false;
                }
            }
            else
            {
                unit.Root.position += to / to.magnitude * step;
            }

            // No headway for a while (bodies in the way push it about): find another way.
            if (Vector3.Distance(unit.Root.position, plan.progressFrom) > 0.2f * Grid.tileSize)
            {
                plan.progressFrom = unit.Root.position;
                plan.progressAt = now;
            }
            else if (now - plan.progressAt > 2f)
            {
                plan.path.Clear();
                plan.hasDest = false;
                plan.progressAt = now;
                if (PlaytestTrace.Enabled)
                {
                    PlaytestTrace.Note(unit, "move", "no headway: finding another way");
                }
            }
        }

        /// <summary>Moves the root toward a point, never further than 35% of a tile from its tile's centre.</summary>
        private void WalkInside(UnitController unit, Vector3 point, float dt, Tile? tile = null)
        {
            var t = tile ?? PlanOf(unit).tile;
            Vector3 center = Grid.Center(t);
            Vector3 to = point - unit.Root.position;
            to.y = 0f;
            float step = Mathf.Min(to.magnitude, unit.WalkSpeed * dt);
            float edge = 0.35f * Grid.tileSize;
            Vector3 here = unit.Root.position - center;
            here.y = 0f;
            if (here.magnitude > edge)
            {
                // (Outside the space it may move about in: straight back toward the middle first.)
                Vector3 back = -here.normalized * Mathf.Min(here.magnitude - edge + 1e-3f, unit.WalkSpeed * dt);
                unit.Root.position += back;
                return;
            }

            Vector3 next = unit.Root.position + (to.sqrMagnitude > 1e-8f ? to.normalized * step : Vector3.zero);
            Vector3 off = next - center;
            off.y = 0f;
            off = Vector3.ClampMagnitude(off, edge);
            unit.Root.position = new Vector3(center.x + off.x, unit.Root.position.y, center.z + off.z);
        }

        /// <summary>A free tile next to the target (the side ones before the corners), the nearest to walk to.</summary>
        private bool BestTile(UnitController unit, UnitController target, Dictionary<long, UnitController> taken, out Tile best)
        {
            var plan = PlanOf(unit);
            var at = TargetTile(target);
            var costs = Costs(plan.tile, taken, unit);
            best = default;
            float bestScore = float.MaxValue;
            for (int ring = 1; ring <= 2 && bestScore == float.MaxValue; ring++)
            {
                for (int dc = -ring; dc <= ring; dc++)
                {
                    for (int dr = -ring; dr <= ring; dr++)
                    {
                        if (Mathf.Max(Mathf.Abs(dc), Mathf.Abs(dr)) != ring)
                        {
                            continue;
                        }

                        var tile = new Tile(at.column + dc, at.row + dr);
                        if (!Grid.Contains(tile) || (taken.TryGetValue(Key(tile), out var who) && who != unit))
                        {
                            continue;
                        }

                        if (!costs.TryGetValue(Key(tile), out float cost))
                        {
                            continue;   // (no way there)
                        }

                        float score = cost + (dc != 0 && dr != 0 ? 0.35f : 0f) + 0.01f * (float)random.NextDouble();
                        if (score < bestScore)
                        {
                            bestScore = score;
                            best = tile;
                        }
                    }
                }
            }

            return bestScore < float.MaxValue;
        }

        private void Route(Plan plan, UnitController unit, Tile dest, Dictionary<long, UnitController> taken, float now, string why)
        {
            plan.routeAt = now;
            plan.path.Clear();
            var costs = Costs(plan.tile, taken, unit, out var from);
            if (!costs.ContainsKey(Key(dest)))
            {
                plan.hasDest = false;
                return;
            }

            var tile = dest;
            while (!SameTile(tile, plan.tile))
            {
                plan.path.Insert(0, tile);
                tile = from[Key(tile)];
            }

            plan.hasDest = plan.path.Count > 0;
            plan.dest = dest;
            plan.progressAt = now;
            plan.progressFrom = unit.Root.position;
            if (plan.hasDest)
            {
                taken[Key(dest)] = unit;   // (held from now on: no one else heads for it)
                if (PlaytestTrace.Enabled)
                {
                    PlaytestTrace.Note(unit, "move", $"{why}: {plan.tile} to {dest}, {plan.path.Count} tiles");
                }
            }
        }

        private Dictionary<long, float> Costs(Tile start, Dictionary<long, UnitController> taken, UnitController unit)
        {
            return Costs(start, taken, unit, out _);
        }

        /// <summary>Walking costs from a tile to every tile it can reach (8 ways; not through others, nor cutting their corners).</summary>
        private Dictionary<long, float> Costs(Tile start, Dictionary<long, UnitController> taken, UnitController unit, out Dictionary<long, Tile> from)
        {
            var cost = new Dictionary<long, float> { [Key(start)] = 0f };
            from = new Dictionary<long, Tile>();
            var open = new List<Tile> { start };
            bool Free(Tile t) => Grid.Contains(t) && (!taken.TryGetValue(Key(t), out var u) || u == unit || u == null);
            while (open.Count > 0)
            {
                int bi = 0;
                for (int i = 1; i < open.Count; i++)
                {
                    if (cost[Key(open[i])] < cost[Key(open[bi])])
                    {
                        bi = i;
                    }
                }

                var t = open[bi];
                open.RemoveAt(bi);
                float c = cost[Key(t)];
                for (int dc = -1; dc <= 1; dc++)
                {
                    for (int dr = -1; dr <= 1; dr++)
                    {
                        if (dc == 0 && dr == 0)
                        {
                            continue;
                        }

                        var n = new Tile(t.column + dc, t.row + dr);
                        if (!Free(n))
                        {
                            continue;
                        }

                        bool diagonal = dc != 0 && dr != 0;
                        if (diagonal && (!Free(new Tile(t.column + dc, t.row)) || !Free(new Tile(t.column, t.row + dr))))
                        {
                            continue;
                        }

                        float nc = c + (diagonal ? 1.414f : 1f);
                        if (!cost.TryGetValue(Key(n), out float old) || nc < old - 1e-4f)
                        {
                            cost[Key(n)] = nc;
                            from[Key(n)] = t;
                            open.Add(n);
                        }
                    }
                }
            }

            return cost;
        }

        /// <summary>Who holds which tile: where each unit stands or lies, and where it is heading.</summary>
        private Dictionary<long, UnitController> Occupancy(IReadOnlyList<UnitController> units)
        {
            var taken = new Dictionary<long, UnitController>();
            foreach (var u in units)
            {
                if (u == null || !u.IsInitialized)
                {
                    continue;
                }

                Vector3 lies = u.Rig.Part(PartId.Pelvis).transform.position;
                if (!u.IsAlive || u.IsDown)
                {
                    // (A body on the board: its tile is in the way.)
                    Claim(taken, Clamp(Grid.TileAt(lies)), u, lies);
                    continue;
                }

                var plan = PlanOf(u);
                Claim(taken, plan.placed ? plan.tile : Clamp(Grid.TileAt(u.Root.position)), u, u.Root.position);
                if (plan.path.Count > 0)
                {
                    taken[Key(plan.path[0])] = u;
                }

                if (plan.hasDest)
                {
                    taken[Key(plan.dest)] = u;
                }
            }

            return taken;
        }

        /// <summary>A tile two claim goes to the one nearer its middle (the other moves off it).</summary>
        private void Claim(Dictionary<long, UnitController> taken, Tile tile, UnitController unit, Vector3 at)
        {
            long key = Key(tile);
            if (taken.TryGetValue(key, out var other) && other != null && other != unit)
            {
                Vector3 c = Grid.Center(tile);
                Vector3 o = other.IsAlive && !other.IsDown ? other.Root.position : other.Rig.Part(PartId.Pelvis).transform.position;
                if (Flat(o - c) <= Flat(at - c))
                {
                    return;
                }
            }

            taken[key] = unit;
        }

        private static float Flat(Vector3 v)
        {
            v.y = 0f;
            return v.magnitude;
        }

        private bool NearestFree(Dictionary<long, UnitController> taken, UnitController unit, out Tile best)
        {
            var plan = PlanOf(unit);
            var costs = Costs(plan.tile, taken, unit);
            best = default;
            float bestCost = float.MaxValue;
            foreach (var pair in costs)
            {
                var tile = new Tile((int)(pair.Key >> 16), (int)(pair.Key & 0xffff));
                if (taken.TryGetValue(pair.Key, out var who) && who != unit)
                {
                    continue;
                }

                float c = pair.Value + 0.5f * Vector3.Distance(Grid.Center(tile), unit.Root.position) / Grid.tileSize;
                if (c < bestCost)
                {
                    bestCost = c;
                    best = tile;
                }
            }

            return bestCost < float.MaxValue;
        }

        // ---- Helpers ------------------------------------------------------------------------------

        private Plan PlanOf(UnitController unit)
        {
            if (!plans.TryGetValue(unit, out var plan))
            {
                plan = new Plan();
                plans[unit] = plan;
            }

            return plan;
        }

        private Tile TargetTile(UnitController target)
        {
            return plans.TryGetValue(target, out var p) && p.placed ? p.tile : Clamp(Grid.TileAt(target.Root.position));
        }

        private bool OnTile(UnitController unit, Tile tile)
        {
            Vector3 d = unit.Root.position - Grid.Center(tile);
            d.y = 0f;
            return d.magnitude <= 0.4f * Grid.tileSize;
        }

        private Tile Clamp(Tile t) => new Tile(Mathf.Clamp(t.column, 0, Grid.columns - 1), Mathf.Clamp(t.row, 0, Grid.rows - 1));

        private static bool Adjacent(Tile a, Tile b) => Mathf.Max(Mathf.Abs(a.column - b.column), Mathf.Abs(a.row - b.row)) <= 1;

        private static bool SameTile(Tile a, Tile b) => a.column == b.column && a.row == b.row;

        private static long Key(Tile t) => ((long)t.column << 16) | (uint)(t.row & 0xffff);

        private static UnitController Nearest(IReadOnlyList<UnitController> units, UnitController unit, bool standingOnly)
        {
            UnitController best = null;
            float bestDistance = float.MaxValue;
            foreach (var other in units)
            {
                if (other == null || other == unit || other.Team == unit.Team || !other.IsInitialized || !other.IsAlive || (standingOnly && other.IsDown))
                {
                    continue;
                }

                float d = (other.Root.position - unit.Root.position).sqrMagnitude * (other.IsDown ? 100f : 1f);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = other;
                }
            }

            return best;
        }

        /// <summary>Chest to chest at medium size (the style's), scaled by both units' sizes.</summary>
        public static float EngageDistance(UnitController unit, UnitController enemy)
        {
            return unit.Style.engageDistance * unit.Body.SizeFactor + 0.25f * enemy.Body.SizeFactor;
        }

        private float Next(CombatStyle style)
        {
            return Mathf.Lerp(style.attackInterval.x, style.attackInterval.y, (float)random.NextDouble());
        }
    }
}
