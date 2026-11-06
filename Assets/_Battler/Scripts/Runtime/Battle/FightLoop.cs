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
    /// The fight loop (milestone 6; free roam since milestone 7). Two teams on the board fight it
    /// out with no help:
    /// <list type="bullet">
    /// <item>Each unit picks a target: the nearest enemy that can still fight, keeping the one it
    /// has, favouring one that is already hurt or already fighting it, and passing over one that
    /// already has <see cref="RingSize"/> attackers.</item>
    /// <item>It walks freely (the tiles are only for placing units before the fight) to a ring round
    /// the target at its striking distance, steered clear of every body near it and of the board's
    /// edge; its captured walk plays. Stuck behind someone, it goes round.</item>
    /// <item>On the ring it keeps its distance (its Fight Rhythm circles it), and when its timer
    /// comes round it strikes any enemy in reach in front of it at a body part a move can land on.
    /// If it reads an opening in the target (a posture's window, a tired guard: <see
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
            public UnitController target;

            /// <summary>The performance pass: the blow being planned a few poses a step, and the step it last went on.</summary>
            public MovePlanner.Job job;
            public int jobStep;
            public AimParts jobPrefer;
            public float retargetAt;
            public float nextAttack = -1f;
            public bool readsThisTime;
            public float ledInAt = -999f;
            public float closeIn;
            public Vector3 velocity;          // the steering's smoothed velocity (m/s)
            public float progressAt;          // when it last got nearer its target's ring
            public float closestOff = float.MaxValue;
            public float detour, detourUntil; // going round someone in the way: which side, until when
            public float blocked;             // seconds it has been trying to walk in and barely moving
            public float blowEnd = -999f;     // when its last blow ends
            public bool hasTempo;             // the tempo state the blow timing was set for
            public float delayAfter;          // the wait after the blow under way (milestone 10: a combo's end isn't known ahead)
            public TempoState tempo;
        }

        /// <summary>The most units of one team on one target at once: more go for another, or wait.</summary>
        public const int RingSize = 3;

        /// <summary>Room kept between two bodies, beyond their own (metres at medium size).</summary>
        public const float Margin = 0.12f;

        /// <summary>More room between two of one team on one target (metres at medium size): to swing in.</summary>
        public const float ElbowRoom = 0.45f;

        /// <summary>How much further (tiles) a target already taken by each teammate counts, to a clip fighter choosing one: they pair off.</summary>
        public const float ClipPairing = 2.5f;

        /// <summary>The more room a clip fighter keeps from any teammate (metres at a Medium's size, on top of the rest): two of a side spread out, each on its own.</summary>
        public const float ClipAllySpread = 0.5f;

        /// <summary>The more room a clip fighter keeps from an enemy it isn't fighting, and that isn't fighting it (metres at a Medium's size): the pairs spread apart.</summary>
        public const float ClipPairSpread = 0.7f;

        /// <summary>A want smaller than this share of the walking speed is dropped (no nervous little steps).</summary>
        public const float DeadZone = 0.12f;

        /// <summary>How fast the steering velocity changes: walking speeds a second.</summary>
        public const float Acceleration = 3f;

        /// <summary>Times a unit made no headway for 2 s and went round (the gate counts them).</summary>
        public int Stalls { get; private set; }

        private readonly Dictionary<UnitController, Plan> plans = new Dictionary<UnitController, Plan>();
        private float fought;
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

        private int stepCount;

        public void Step(IReadOnlyList<UnitController> units, float dt, float now)
        {
            stepCount++;
            MovePlanner.BeginStep(stepCount);
            if (StartedAt < 0f && Fight)
            {
                StartedAt = now;
            }

            if (Fight && !Over && StartedAt >= 0f)
            {
                fought += dt;   // (the time limit counts fighting time only)
            }

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
                if (unit.UsesClips)
                {
                    // (Stood still unless the steering below asks it to walk.)
                    unit.ClipWant = Vector3.zero;
                }

                if (!unit.IsAlive || !unit.CanAct)
                {
                    // Off its feet or gone: it doesn't steer (a body on the board is in others' way).
                    plan.velocity = Vector3.zero;
                    plan.progressAt = now;
                    continue;
                }

                // (Free roam: anywhere on the board is a spot; the steering keeps it clear of others.)
                unit.NeedsSpot = false;

                if (!Fight)
                {
                    // (Not fighting: it stays where it was put, facing the nearest enemy.)
                    var near = Nearest(units, unit, false);
                    unit.Opponent = near;
                    unit.CircleOpponent = false;
                    if (near != null)
                    {
                        unit.FacePoint(near.Root.position);
                    }

                    plan.velocity = Vector3.zero;
                    plan.progressAt = now;
                    continue;
                }

                if (Over || !CanFight(unit))
                {
                    // (Out of it, or it's over: it stands, kept clear of others, facing the nearest enemy.)
                    var near = Nearest(units, unit, false);
                    unit.Opponent = null;
                    unit.CircleOpponent = false;
                    Steer(units, unit, plan, Vector3.zero, dt);
                    if (near != null)
                    {
                        unit.FacePoint(near.Root.position);
                    }

                    continue;
                }

                var target = ChooseTarget(units, unit, plan, now);
                unit.Opponent = target;
                if (target == null)
                {
                    unit.CircleOpponent = false;
                    Steer(units, unit, plan, Vector3.zero, dt);
                    Search(units, unit);
                    continue;
                }

                Approach(units, unit, target, plan, dt, now);
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
            else if (fought > TimeLimit)
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
            Result = (winner < 0 ? "a draw" : $"team {(winner == 0 ? "A" : "B")} wins") + $" after {fought:0} s ({why})";
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
            bool valid = current != null && current.IsInitialized && current.IsAlive && current.Team != unit.Team && !current.IsDown && unit.Knows(current);
            if (valid && now < plan.retargetAt)
            {
                return current;
            }

            plan.retargetAt = now + 0.5f;
            UnitController best = null;
            float bestScore = float.MaxValue;
            foreach (var other in units)
            {
                // (Milestone 9: only an enemy it knows is there: seen, felt or remembered.)
                if (other == null || !other.IsInitialized || !other.IsAlive || other.Team == unit.Team || !unit.Knows(other))
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

                // (Its attack ring full: others are on it already. It goes for another, or waits.)
                int on = Attackers(units, other, unit);
                if (on >= RingSize)
                {
                    score += 3f;
                }

                // (A clip fighter pairs off (the playtest of 6 November: they clumped up, four on one spot): one already
                // taken by a teammate counts as further, so they spread over the enemies, two on one only when the numbers
                // make it so.)
                if (unit.UsesClips)
                {
                    score += ClipPairing * on;
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

        /// <summary>
        /// Milestone 9: no enemy it knows of, though some are still fighting: it turns about to find
        /// one (toward where it last knew of one, or toward the middle of the board).
        /// </summary>
        private void Search(IReadOnlyList<UnitController> units, UnitController unit)
        {
            bool any = false;
            foreach (var other in units)
            {
                any |= other != null && other.IsInitialized && other.IsAlive && other.Team != unit.Team && !other.IsDown;
            }

            if (!any)
            {
                return;   // (only enemies on the ground: it waits for one to get up)
            }

            // Turned well round each step: it keeps turning that way until something comes into view.
            Vector3 ahead = Quaternion.Euler(0f, unit.FacingYaw + 60f, 0f) * Vector3.forward;
            unit.FacePoint(unit.Root.position + ahead);
        }

        // ---- Fighting -----------------------------------------------------------------------------

        /// <summary>
        /// Free roam toward the target: it walks to a point on a ring round it at its striking
        /// distance (the side it comes from; others on the ring and every body nearby push it along),
        /// keeps that distance once there, and strikes when its timer comes round.
        /// </summary>
        private void Approach(IReadOnlyList<UnitController> units, UnitController unit, UnitController enemy, Plan plan, float dt, float now)
        {
            float size = unit.Body.SizeFactor;
            Vector3 to = enemy.Root.position - unit.Root.position;
            to.y = 0f;
            float distance = to.magnitude;
            float closest = 0.45f * size + 0.2f * enemy.Body.SizeFactor;
            float engage = Mathf.Max(closest, EngageDistance(unit, enemy) - plan.closeIn);
            Vector3 toward = to / Mathf.Max(1e-4f, distance);
            float flankLeft = 0f;
            if (unit.UsesClips)
            {
                toward = Flank(units, unit, enemy, toward, engage, out flankLeft);
            }

            float speed = unit.WalkSpeed;
            bool busy = unit.IsAttacking || unit.HitStop > 0f || unit.Knockback.magnitude >= 0.05f;

            // Milestone 8: its tempo sets the distance. Gathering, it holds just out of the target's
            // reach; shelled up, it gives ground; pressing, it closes right in.
            var tempo = unit.TempoOn ? unit.Tempo : null;
            var state = tempo != null ? tempo.State : TempoState.Probe;
            float enemyReach = EngageDistance(enemy, unit);
            unit.TempoEngaged = distance <= Mathf.Max(engage, enemyReach) + 0.8f * size;
            unit.TargetVulnerable = enemy.State != UnitState.Standing || enemy.Health < 0.25f * enemy.Profile.maxHealth;
            bool holdingBack = state == TempoState.Gather || state == TempoState.Shell;
            switch (state)
            {
                case TempoState.Gather:
                    engage = Mathf.Max(engage, enemyReach) + 0.35f * size;
                    break;
                case TempoState.Shell:
                    engage += 0.2f * size + unit.GiveGround;   // (milestone 11: and the ground it gives under each blow)
                    break;
                case TempoState.Press:
                    engage = Mathf.Max(closest, engage - 0.1f * size);
                    break;
            }

            // Toward the ring, slowing over the last stretch; out again if it is too close.
            Vector3 desired = Vector3.zero;
            float off = distance - engage;
            if (off > 0.05f)
            {
                desired = toward * speed * Mathf.Clamp01(off / (0.4f * size));
                // No headway for a while (someone in the way): round them, one side for a moment.
                if (now < plan.detourUntil)
                {
                    desired += Vector3.Cross(Vector3.up, toward) * (plan.detour * 0.8f * speed);
                }
            }
            else if (off < -0.05f && (plan.closeIn < 0f || off < -0.4f * size || holdingBack))
            {
                // (Only when its blows can't land from this close, it is really crowded, or it is
                // holding back: a longer reach doesn't keep backing off from a shorter one, or the
                // shorter could never get in. Shelled up, it gives ground too: milestone 11 keeps it further off under each blow.)
                desired = -toward * 0.6f * speed * Mathf.Clamp01(-off / (0.3f * size));
            }

            // (Flanking: at its striking distance but not yet round at its side, it goes on round.)
            if (flankLeft > 0.25f * size && desired.sqrMagnitude < 0.01f * speed * speed)
            {
                desired = toward * (0.7f * speed * Mathf.Clamp01(flankLeft / (0.4f * size)));
            }

            bool near = off <= 0.3f * size;
            unit.CircleOpponent = near && !busy;
            if (busy)
            {
                plan.velocity = Vector3.zero;
            }
            else
            {
                Steer(units, unit, plan, desired, dt);
            }

            // Facing: the target once it is close, else the way it walks.
            if (distance < 2.6f * Grid.tileSize || plan.velocity.sqrMagnitude < 0.01f)
            {
                unit.FacePoint(enemy.Root.position);
            }
            else
            {
                unit.FacePoint(unit.Root.position + plan.velocity);
            }

            // Headway: far from the ring and not getting closer for 2 s.
            if (off < 0.3f * size || plan.closestOff - off > 0.2f * size)
            {
                plan.closestOff = off;
                plan.progressAt = now;
            }
            else if (now - plan.progressAt > 2f)
            {
                plan.detour = random.NextDouble() < 0.5 ? -1f : 1f;
                plan.detourUntil = now + 1.5f;
                plan.progressAt = now;
                plan.closestOff = off;
                Stalls++;
                if (PlaytestTrace.Enabled)
                {
                    PlaytestTrace.Note(unit, "move", $"no headway toward {enemy.name} ({distance:0.0} m): going round to the {(plan.detour > 0f ? "left" : "right")}");
                }
            }

            // Too far to strike: it walks in. Nearly there, it tries anyway (a lunge may cover the rest;
            // if no blow lands, the planner sends it closer); held up for a moment, it tries from there.
            plan.blocked = off > 0.05f && plan.velocity.magnitude < 0.25f * speed ? plan.blocked + dt : 0f;
            if (busy || (off > 0.35f * size && plan.blocked < 0.6f) || (tempo != null && !tempo.Strikes))
            {
                return;
            }

            // (Step 4 of the plan: clip fighters only move; their blows come with step 5.)
            if (unit.UsesClips && !UnitController.ClipBlows)
            {
                return;
            }

            Attack(units, unit, enemy, plan, distance, now);
        }

        /// <summary>
        /// A clip fighter's way in to a target a teammate is on already (the playtest of 6 November: the second came in
        /// behind the first, stuck in its back): round to a side of the target that's free, as far round from each of them
        /// as <see cref="FlankApart"/> asks. The way to that place at the striking distance, from where it stands.
        /// </summary>
        private Vector3 Flank(IReadOnlyList<UnitController> units, UnitController unit, UnitController enemy, Vector3 toward, float engage, out float left)
        {
            left = 0f;
            Vector3 from = Flat(unit.Root.position - enemy.Root.position);
            if (from.sqrMagnitude < 1e-4f)
            {
                return toward;
            }

            from.Normalize();
            float turn = 0f;
            foreach (var other in units)
            {
                if (other == null || other == unit || other.Team != unit.Team || !other.IsInitialized || !other.IsAlive || other.IsDown
                    || !plans.TryGetValue(other, out var theirs) || theirs.target != enemy)
                {
                    continue;
                }

                Vector3 theirFrom = Flat(other.Root.position - enemy.Root.position);
                if (theirFrom.sqrMagnitude < 1e-4f)
                {
                    continue;
                }

                // (Its side of the target against theirs: nearer than FlankApart round, it goes further round, away from them.)
                float angle = Vector3.SignedAngle(theirFrom, from, Vector3.up);
                if (Mathf.Abs(angle) < FlankApart)
                {
                    float side = Mathf.Abs(angle) < 1f ? (unitsOrder(unit) < unitsOrder(other) ? 1f : -1f) : Mathf.Sign(angle);
                    turn += side * (FlankApart - Mathf.Abs(angle));
                }
            }

            if (Mathf.Abs(turn) < 1f)
            {
                return toward;
            }

            Vector3 place = enemy.Root.position + Quaternion.Euler(0f, Mathf.Clamp(turn, -FlankApart, FlankApart), 0f) * from * engage;
            Vector3 way = Flat(place - unit.Root.position);
            left = way.magnitude;
            return way.sqrMagnitude > 1e-4f ? way.normalized : toward;

            int unitsOrder(UnitController u)
            {
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i] == u)
                    {
                        return i;
                    }
                }

                return 0;
            }
        }

        /// <summary>Two of a side on one target stand at least this far round it from each other (degrees).</summary>
        public const float FlankApart = 100f;

        /// <summary>In reach: when its timer comes round (or an opening it reads), a blow at the target or another enemy in front of it.</summary>
        private void Attack(IReadOnlyList<UnitController> units, UnitController unit, UnitController enemy, Plan plan, float distance, float now)
        {
            var style = unit.Style;
            float size = unit.Body.SizeFactor;
            var tempo = unit.TempoOn ? unit.Tempo : null;
            // Milestone 10: while a blow or a combo plays, the next waits until it is over (a combo's
            // links come from the combo itself).
            if (unit.IsAttacking || unit.Combo != null)
            {
                plan.job = null;
                plan.blowEnd = now;
                plan.nextAttack = Mathf.Max(plan.nextAttack, now + plan.delayAfter);
                return;
            }

            // (The performance pass: a blow being planned carries on, a few poses a step; one left off
            // (the unit was busy, knocked about) is dropped.)
            if (plan.job != null)
            {
                if (stepCount - plan.jobStep > 2)
                {
                    plan.job = null;
                }
                else
                {
                    plan.jobStep = stepCount;
                    if (plan.job.Step())
                    {
                        FinishPlanning(unit, enemy, plan, distance, now, style, tempo, size);
                    }

                    return;
                }
            }

            if (tempo != null)
            {
                // Milestone 8: the tempo times the blows. Each time its state changes, the next blow
                // comes the state's delay after the last one ends (a press quickly, a probe in a while,
                // a counter at once).
                if (!plan.hasTempo || plan.tempo != tempo.State)
                {
                    plan.hasTempo = true;
                    plan.tempo = tempo.State;
                    plan.nextAttack = Mathf.Max(now, plan.blowEnd) + tempo.BlowDelay((float)random.NextDouble());
                    plan.readsThisTime = random.NextDouble() < style.readsOpenings;
                }
            }
            else if (plan.nextAttack < 0f)
            {
                plan.nextAttack = now + Next(style) * 0.5f;
                plan.readsThisTime = random.NextDouble() < style.readsOpenings;
                return;
            }

            // An opening it reads: strike into it now if the blow was coming soon anyway (milestone 9:
            // only one it sees clearly).
            var open = unit.Sees(enemy) == Sight.Clear ? enemy.Openings : AimParts.None;
            bool opening = plan.readsThisTime && open != AimParts.None && now >= plan.nextAttack - style.openingLead;
            if ((now < plan.nextAttack && !opening) || unit.Stance.AnyStepping)
            {
                return;
            }

            // Its Fight Rhythm may lead into the blow first (not into an opening: that would waste it;
            // in a press only before its first blow; never before a counter).
            bool leads = tempo == null || tempo.State == TempoState.Probe || (tempo.State == TempoState.Press && tempo.BurstBlows == 0);
            if (!opening && leads && now - plan.ledInAt > 1.5f && !unit.UsesClips)
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

            if (unit.UsesClips)
            {
                // (The plan "Bringing the lab into the fight", step 5: a clip fighter strikes with the lab's strikes, a
                // combo when the fight would throw one, at its target first, then the others in reach in front of it.)
                foreach (var target in candidates)
                {
                    var combo = ChooseCombo(unit, tempo, target, out _, out _);
                    if (unit.ClipStrike(target, combo != null))
                    {
                        plan.ledInAt = -999f;
                        plan.closeIn = 0f;
                        plan.blowEnd = now + 1f;
                        plan.delayAfter = tempo != null ? tempo.BlowDelay((float)random.NextDouble()) : Next(style);
                        plan.nextAttack = plan.blowEnd + plan.delayAfter;
                        plan.readsThisTime = random.NextDouble() < style.readsOpenings;
                        return;
                    }
                }

                // (None of its strikes reaches: too far, it closes in; too close, it gives ground.)
                bool tooClose = distance < EngageDistance(unit, enemy) - 0.1f * size;
                plan.closeIn = Mathf.Clamp(plan.closeIn + (tooClose ? -0.15f : 0.15f) * size, -0.5f * size, 0.9f * size);
                plan.nextAttack = now + 0.4f;
                if (PlaytestTrace.Enabled)
                {
                    PlaytestTrace.Note(unit, "attack", $"none of its strikes reaches {enemy.name} from {distance:0.00} m; {(tooClose ? "backing off" : "closing in")}");
                }

                return;
            }

            var targets = new List<(UnitController, AimParts)>();
            foreach (var target in candidates)
            {
                var prefer = target == enemy && plan.readsThisTime ? open : AimParts.None;
                targets.Add((target, prefer));

                // Milestone 10: a combo, when the tempo and the energy call for one.
                var combo = ChooseCombo(unit, tempo, target, out int length, out string why);
                if (combo != null)
                {
                    var first = unit.PlanLink(combo, 0, target, random);
                    if (first != null && unit.StartCombo(combo, length, target, first, why))
                    {
                        plan.ledInAt = -999f;
                        plan.closeIn = 0f;
                        plan.blowEnd = now + first.Duration;
                        plan.delayAfter = tempo != null ? tempo.BlowDelay((float)random.NextDouble()) : Next(style);
                        plan.nextAttack = plan.blowEnd + plan.delayAfter;
                        plan.readsThisTime = random.NextDouble() < style.readsOpenings;
                        return;
                    }
                }
            }

            // The blow, at its target first, then the others (the performance pass: planned a few
            // poses a step, so no step carries all of it).
            plan.job = new MovePlanner.Job(unit, targets, random, 4f);
            plan.jobStep = stepCount;
            plan.jobPrefer = plan.readsThisTime ? open : AimParts.None;
            if (plan.job.Step())
            {
                FinishPlanning(unit, enemy, plan, distance, now, style, tempo, size);
            }
        }

        /// <summary>A blow planned: thrown (or, if nothing lands, the unit closes in or gives ground).</summary>
        private void FinishPlanning(UnitController unit, UnitController enemy, Plan plan, float distance, float now, CombatStyle style, TempoDirector tempo, float size)
        {
            var job = plan.job;
            plan.job = null;
            var state = job.Result;
            var target = job.Target;
            if (state != null && target != null && target.IsAlive && unit.CanAct && !unit.IsAttacking)
            {
                var prefer = target == enemy ? plan.jobPrefer : AimParts.None;
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

                plan.blowEnd = now + state.Duration;
                plan.delayAfter = tempo != null ? tempo.BlowDelay((float)random.NextDouble()) : Next(style);
                plan.nextAttack = plan.blowEnd + plan.delayAfter;
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

        /// <summary>How likely a press with the energy for two links throws a combo rather than a single blow; a counter; a target reeling; the first blow out of a full gather (its finisher).</summary>
        public const float ComboPressChance = 0.09f, ComboCounterChance = 0.12f, ComboPunishChance = 0.25f, ComboFullGatherChance = 0.6f;

        /// <summary>
        /// Milestone 10: when combos come out. Pressing with the energy for at least two links (the
        /// main source); a counter (a short string); a target reeling (a long string with its
        /// finisher); and fresh out of a full gather, the longest string the style has. Length: the
        /// least of the string's, the energy over the cost of a link, and two plus the aggression's
        /// share of two more.
        /// </summary>
        private ComboDefinition ChooseCombo(UnitController unit, TempoDirector tempo, UnitController target, out int length, out string why)
        {
            length = 0;
            why = null;
            if (tempo == null || unit.Rhythm == null || unit.Loadout?.Main == null || target == null)
            {
                return null;
            }

            var strings = ComboDefinition.For(unit.Loadout.Main, unit.Body.SizeFactor);
            if (strings.Count == 0)
            {
                return null;
            }

            float energy = unit.Rhythm.Energy, cost = Mathf.Max(0.01f, tempo.BlowCost);
            bool finisher = false;
            float chance;
            int most = 99;
            if (unit.TargetVulnerable && tempo.Strikes)
            {
                why = "punish";
                chance = ComboPunishChance;
                finisher = true;
            }
            else if (tempo.State == TempoState.Counter)
            {
                why = "counter";
                chance = ComboCounterChance;
                most = 2;
            }
            else if (tempo.State == TempoState.Press)
            {
                bool fresh = tempo.BurstBlows == 0 && energy >= tempo.Temperament.fullCharge;
                why = fresh ? "full gather" : "press";
                chance = fresh ? ComboFullGatherChance : ComboPressChance;
                finisher = fresh;
            }
            else
            {
                return null;
            }

            if (random.NextDouble() >= chance)
            {
                return null;
            }

            int afford = Mathf.FloorToInt(energy / cost + 1e-3f);
            int temper = 2 + Mathf.RoundToInt(2f * Mathf.Clamp01(unit.Rhythm.Aggression));
            var pool = strings.FindAll(c => c.finisher == finisher);
            if (pool.Count == 0)
            {
                pool = strings;
            }

            var combo = pool[random.Next(pool.Count)];
            length = Mathf.Min(combo.links.Length, Mathf.Min(afford, Mathf.Min(temper, most)));
            if (finisher)
            {
                length = Mathf.Min(combo.links.Length, afford);
            }

            return length >= 2 ? combo : null;
        }

        // ---- Steering --------------------------------------------------------------------------------

        /// <summary>How many standing units of the other team have this one as their target (not counting one).</summary>
        private int Attackers(IReadOnlyList<UnitController> units, UnitController target, UnitController except)
        {
            int n = 0;
            foreach (var u in units)
            {
                if (u != null && u != except && u.IsInitialized && u.IsAlive && u.Team != target.Team && plans.TryGetValue(u, out var p) && p.target == target)
                {
                    n++;
                }
            }

            return n;
        }

        /// <summary>The space a standing unit takes on the board (metres): its shoulders' half-width, about.</summary>
        public static float BodyRadius(UnitController unit) => 0.28f * unit.Body.SizeFactor;

        /// <summary>Where a unit's body is on the board: the root standing, the hips lying.</summary>
        public static Vector3 BodyAt(UnitController unit)
        {
            Vector3 p = unit.IsAlive && !unit.IsDown ? unit.Root.position : unit.PartPosition(PartId.Pelvis);
            p.y = 0f;
            return p;
        }

        /// <summary>Pairs of standing units whose bodies overlap (closer than <paramref name="share"/> of their two radii together).</summary>
        public static int Overlapping(IReadOnlyList<UnitController> units, float share = 0.8f)
        {
            int n = 0;
            for (int i = 0; i < units.Count; i++)
            {
                var a = units[i];
                if (a == null || !a.IsInitialized || !a.IsAlive || a.State != UnitState.Standing)
                {
                    continue;
                }

                for (int j = i + 1; j < units.Count; j++)
                {
                    var b = units[j];
                    if (b == null || !b.IsInitialized || !b.IsAlive || b.State != UnitState.Standing)
                    {
                        continue;
                    }

                    if (Vector3.Distance(BodyAt(a), BodyAt(b)) < share * (BodyRadius(a) + BodyRadius(b)))
                    {
                        n++;
                    }
                }
            }

            return n;
        }

        /// <summary>
        /// Moves the root by a smoothed velocity: what it wants (toward its target, say), plus a push
        /// away from every body too close (standing or lying, either team) and back from the board's
        /// edge. Small wants are dropped, so the feet don't take nervous little steps.
        /// </summary>
        private void Steer(IReadOnlyList<UnitController> units, UnitController unit, Plan plan, Vector3 desired, float dt)
        {
            float speed = unit.WalkSpeed;
            Vector3 here = BodyAt(unit);
            float mine = BodyRadius(unit);
            Vector3 push = Vector3.zero;
            foreach (var other in units)
            {
                if (other == null || other == unit || !other.IsInitialized)
                {
                    continue;
                }

                if (other.Lying)
                {
                    // Milestone 9b: a body on the ground is walked round (its torso and legs), gently,
                    // and stepped across only when there's no other way.
                    push += AroundBody(here, mine, other, speed);
                    continue;
                }

                Vector3 d = here - BodyAt(other);
                float room = mine + BodyRadius(other) + Margin * unit.Body.SizeFactor;
                if (unit.UsesClips)
                {
                    // (A clip fighter's walk answers a push late, its feet taking the steps: it starts sooner; and it keeps off
                    // its own side, each fighting its own.)
                    room += ClipRoom * unit.Body.SizeFactor;
                    if (other.Team == unit.Team)
                    {
                        room += ClipAllySpread * unit.Body.SizeFactor;
                    }
                    else if (other != plan.target && other.Opponent != unit)
                    {
                        // (An enemy fighting someone else: the next pair over, kept off.)
                        room += ClipPairSpread * unit.Body.SizeFactor;
                    }
                }

                if (other.Team == unit.Team && plans.TryGetValue(other, out var theirs) && theirs.target != null && theirs.target == plan.target)
                {
                    room += ElbowRoom * unit.Body.SizeFactor;   // (two on one target: room to swing without hitting each other)
                }

                float apart = d.magnitude;
                if (apart >= room)
                {
                    continue;
                }

                Vector3 away = apart > 1e-4f ? d / apart : Vector3.Cross(Vector3.up, unit.transform.forward);
                push += away * (1.5f * speed * (room - apart) / room);
            }

            // The board's edge.
            float halfW = 0.5f * Grid.Width - mine, halfD = 0.5f * Grid.Depth - mine;
            Vector3 local = here - Grid.origin;
            if (local.x > halfW) push.x -= speed * Mathf.Clamp01((local.x - halfW) / 0.2f);
            if (local.x < -halfW) push.x += speed * Mathf.Clamp01((-halfW - local.x) / 0.2f);
            if (local.z > halfD) push.z -= speed * Mathf.Clamp01((local.z - halfD) / 0.2f);
            if (local.z < -halfD) push.z += speed * Mathf.Clamp01((-halfD - local.z) / 0.2f);

            Vector3 want = Vector3.ClampMagnitude(desired + push, speed);
            if (want.magnitude < DeadZone * speed)
            {
                want = Vector3.zero;
            }

            plan.velocity = Vector3.MoveTowards(plan.velocity, want, Acceleration * speed * dt);
            if (unit.UsesClips)
            {
                // (Being pushed off a body, it doesn't circle on into it.)
                unit.ClipCrowded = Mathf.Clamp01(push.magnitude / Mathf.Max(0.05f, speed));
                // (The plan "Bringing the lab into the fight", step 4: a clip fighter walks there itself, its feet doing
                // the stepping, its walk easing into what it is asked (so not eased here as well: a push off a body
                // came too late, and teammates on one target overlapped); its root follows its walk.)
                plan.velocity = want;
                unit.ClipWant = want;
                return;
            }

            Vector3 next = unit.Root.position + plan.velocity * dt;
            // (Never off the board, whatever pushed it.)
            Vector3 n = next - Grid.origin;
            n.x = Mathf.Clamp(n.x, -0.5f * Grid.Width + 0.5f * mine, 0.5f * Grid.Width - 0.5f * mine);
            n.z = Mathf.Clamp(n.z, -0.5f * Grid.Depth + 0.5f * mine, 0.5f * Grid.Depth - 0.5f * mine);
            unit.Root.position = new Vector3(Grid.origin.x + n.x, next.y, Grid.origin.z + n.z);
        }

        /// <summary>The more room a clip fighter keeps from other bodies (metres at a Medium's size): its walk answers late.</summary>
        public const float ClipRoom = 0.2f;

        /// <summary>How far round a body on the ground a walking unit keeps (metres at medium size, beyond its own radius).</summary>
        public const float BodyClearance = 0.12f;

        /// <summary>The push away from a body lying on the ground: from its head to its hips and its hips to each foot.</summary>
        public static Vector3 AroundBody(Vector3 here, float mine, UnitController lying, float speed)
        {
            Vector3 pelvis = Flat(lying.PartPosition(PartId.Pelvis));
            float room = mine + BodyClearance * lying.Body.SizeFactor;
            Vector3 push = Vector3.zero;
            foreach (var end in new[] { PartId.Head, PartId.FootL, PartId.FootR })
            {
                if (lying.Rig.IsSevered(end))
                {
                    continue;
                }

                Vector3 b = Flat(lying.PartPosition(end));
                Vector3 ab = b - pelvis;
                float t = ab.sqrMagnitude > 1e-6f ? Mathf.Clamp01(Vector3.Dot(here - pelvis, ab) / ab.sqrMagnitude) : 0f;
                Vector3 d = here - (pelvis + ab * t);
                float apart = d.magnitude;
                if (apart >= room)
                {
                    continue;
                }

                Vector3 away = apart > 1e-4f ? d / apart : Vector3.Cross(Vector3.up, ab.sqrMagnitude > 1e-6f ? ab.normalized : Vector3.forward);
                push += away * (speed * (room - apart) / room);
            }

            return push;
        }

        private static Vector3 Flat(Vector3 p) => new Vector3(p.x, 0f, p.z);

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
