using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// Plays a combo (the combat plan's step 5): its first strike aimed, then each next one taken up where
    /// its link has it (<see cref="StrikeLink"/>), aimed afresh, when the last one reaches its link moment,
    /// or ends early (stopped by what it met: the combo goes on from where it backed out).
    ///
    /// With <see cref="Strikes"/> given, the next strike is chosen there and then, as the old move planner
    /// chose a move only if it could land: of the follow-ons in order of their cost, the first that lands on
    /// its target from where the link leaves the body (<see cref="LocoBody.MissAfterPull"/> within
    /// <see cref="LandWithin"/>), a follow-on that needs the body turned less onto its target before one that
    /// needs it turned more (<see cref="TurnWeight"/>: a strike swung with the body turned well off the way it
    /// was played looks it). None lands (the target driven back by the last blow), the combo presses: when the
    /// strike is over it steps in after the target in its guard (<see cref="Want"/>, for whoever moves the walk)
    /// until one would land, and goes on with it; for at most <see cref="PressMost"/>, then it ends.
    /// </summary>
    public sealed class ComboPlayer
    {
        private readonly Locomotor loco;
        private readonly LocoBody poser;

        /// <summary>Where each strike is aimed, as it starts.</summary>
        public Func<LocoClip, Vector3> TargetFor;

        /// <summary>Each strike as it starts (the first's link is null).</summary>
        public event Action<LocoClip, StrikeLink> Started;

        private List<StrikeLink> chain = new List<StrikeLink>();
        private int next;
        private LocoClip current;

        /// <summary>The strikes a combo may go on with (null: the chain given to <see cref="Start"/> as it is).</summary>
        public IList<LocoClip> Strikes;

        /// <summary>The most strikes in a combo, and how near a follow-on must reach its target to be chosen (metres).</summary>
        public int Most = 3;
        public float LandWithin = 0.05f, MostCost = 1.2f;

        /// <summary>What turning the body a full <see cref="LocoBody.MostAimTurn"/> onto the target adds to a follow-on's cost.</summary>
        public float TurnWeight = 0.4f;

        /// <summary>Pressing: the longest it steps in after the target (seconds), and how fast (m/s at a Medium's size).</summary>
        public float PressMost = 3f, PressSpeed = 1.5f;

        /// <summary>Pressing, a follow-on is taken up once the body needs turning at most this onto the target (degrees), for the first half of the pressing.</summary>
        public float PressTurn = 60f;

        /// <summary>Pressing, it stops stepping in once the nearest follow-on would miss by no more than this past landing (metres): the plan's lunge and pull take up the rest.</summary>
        public float StopWithin = 0.15f;

        /// <summary>Stepping in after the target (the strike over, no follow-on landing from where it is).</summary>
        public bool Pressing { get; private set; }

        /// <summary>How the walk should move while pressing (world, m/s; zero otherwise), and where it should face (degrees).</summary>
        public Vector3 Want { get; private set; }
        public float WantFacing { get; private set; }

        private float pressedFor, shotTime;

        /// <summary>The follow-ons weighed for the last strike chosen at a link (for the logs).</summary>
        public string LastChoice = "";

        private readonly System.Text.StringBuilder why = new System.Text.StringBuilder();

        private readonly HashSet<LocoClip> used = new HashSet<LocoClip>();
        private int played;

        /// <summary>Why the combo ended where it did ("" while it goes on).</summary>
        public string Ended = "";

        public ComboPlayer(Locomotor loco, LocoBody poser)
        {
            this.loco = loco;
            this.poser = poser;
        }

        /// <summary>A combo is under way.</summary>
        public bool Playing => current != null;

        public IReadOnlyList<StrikeLink> Chain => chain;

        public void Start(LocoClip first, List<StrikeLink> links)
        {
            chain = links ?? new List<StrikeLink>();
            next = 0;
            current = first;
            shotTime = 0f;
            used.Clear();
            used.Add(first);
            played = 1;
            Ended = "";
            Pressing = false;
            Want = Vector3.zero;
            poser.PlayStrike(first, Target(first));
            Started?.Invoke(first, null);
        }

        public void Stop()
        {
            current = null;
            Pressing = false;
            loco.Hurry = false;
            Want = Vector3.zero;
        }

        /// <summary>How the target is moving (world, m/s; null: it stands): a strike is aimed where it will be.</summary>
        public Func<Vector3> TargetVelocity;

        /// <summary>How much of the target's way it is led by, at most how far ahead (seconds): a driven body slows.</summary>
        public const float LeadShare = 0.6f, MostLead = 0.6f;

        private Vector3 Target(LocoClip clip, float startFrom = 0f)
        {
            Vector3 at = TargetFor != null ? TargetFor(clip) : poser.NominalTarget();
            if (TargetVelocity != null && clip != null && clip.ContactFrame >= 0)
            {
                Vector3 v = TargetVelocity();
                v.y = 0f;
                at += v * (LeadShare * Mathf.Clamp(clip.ContactFrame / clip.Rate - startFrom, 0f, MostLead));
            }

            return at;
        }

        /// <summary>The follow-ons of the strike playing (not yet used, cost at most <see cref="MostCost"/>).</summary>
        private List<StrikeLink> Options()
        {
            var options = new List<StrikeLink>();
            foreach (var to in Strikes)
            {
                if (!used.Contains(to))
                {
                    var link = StrikeCombos.Link(current, to);
                    if (link.Cost <= MostCost)
                    {
                        options.Add(link);
                    }
                }
            }

            return options;
        }

        private void Go(StrikeLink link)
        {
            loco.TurnRoot(link.Turn, Quaternion.Euler(0f, loco.Facing, 0f) * link.Shift * (loco.Leg / 0.845f));
            poser.PlayStrike(link.To, Target(link.To, link.Entry), startFrom: link.Entry);
            current = link.To;
            shotTime = link.Entry;
            used.Add(link.To);
            played++;
            Started?.Invoke(link.To, link);
        }

        /// <summary>
        /// The follow-ons of the strike playing that would land from where its link leaves the body, best first;
        /// the time to go is the earliest of their links (or the strike's end).
        /// </summary>
        private void StepChoosing(bool still)
        {
            if (played >= Most)
            {
                if (!still)
                {
                    current = null;
                }

                return;
            }

            // (Each follow-on's link has its own moment: the strike goes on until the first of them.)
            var options = Options();
            if (!still && options.Count > 0)
            {
                // (Over before its links' moments, cut short by what it met: the follow-ons are taken up from where
                // it stopped, the body turned and placed as it is there, not as it would have been at its link mark.)
                for (int i = 0; i < options.Count; i++)
                {
                    if (shotTime + 1e-3f < options[i].At)
                    {
                        var cut = StrikeCombos.BestAt(current, shotTime, options[i].To);
                        options[i] = cut;
                    }
                }

                float cheapest = float.MaxValue;
                string cheapestName = "";
                foreach (var l in options)
                {
                    if (l.Cost < cheapest)
                    {
                        cheapest = l.Cost;
                        cheapestName = l.To.Name;
                    }
                }

                options.RemoveAll(l => l.Cost > MostCost);
                if (options.Count == 0)
                {
                    // (Stopped mid-blow, its pose meets no follow-on's wind-up: it backs out into its guard, and the
                    // combo goes on from there, as pressing does, facing the target first.)
                    Pressing = true;
                    pressedFor = 0f;
                    Ended = $"cut short at {shotTime:0.00} s: no follow-on meets it there (the nearest, {cheapestName}, costs {cheapest:0.00}); the next is taken up from the guard";
                    return;
                }
            }

            if (options.Count == 0)
            {
                Ended = "no follow-on";
                if (!still)
                {
                    current = null;
                }

                return;
            }

            float scale = loco.Leg / 0.845f;
            why.Clear();
            StrikeLink best = null, nearest = null;
            float nearestMiss = float.MaxValue, bestScore = float.MaxValue;
            foreach (var link in options)
            {
                // (From where the link leaves the root: turned and moved there.)
                Vector3 rootAt = loco.Position + Quaternion.Euler(0f, loco.Facing, 0f) * link.Shift * scale;
                float facing = loco.Facing + link.Turn;
                var target = Target(link.To, link.Entry);
                float miss = poser.MissAfterPull(link.To, target, link.Entry, rootAt, facing);
                if (miss < nearestMiss)
                {
                    nearestMiss = miss;
                    nearest = link;
                }

                float turnNeeded = poser.AimTurnFor(link.To, target, link.Entry, rootAt, facing);
                why.Append($"{link.To.Name.Replace("sns_", "")} cost {link.Cost:0.00} miss {miss:0.00} turn {turnNeeded:+0;-0}; ");
                if (miss <= LandWithin)
                {
                    float turn = Mathf.Abs(turnNeeded);
                    float score = link.Cost + TurnWeight * turn / Mathf.Max(1f, poser.MostAimTurn);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = link;
                    }
                }
            }

            if (best == null)
            {
                // (None lands from here: the strike plays on (the body moves); at its end the combo presses.)
                if (still)
                {
                    return;
                }

                Pressing = true;
                pressedFor = 0f;
                Ended = $"no follow-on would land from where the strike left it (the nearest, {nearest?.To.Name}, would miss by {nearestMiss:0.00} m)";
                return;
            }

            if (still && loco.ShotTime < best.At)
            {
                return;
            }

            LastChoice = why.ToString();

            Go(best);
        }

        /// <summary>
        /// Pressing: stepping in after the target in the guard until a follow-on of the last strike would land from
        /// where the body is, played from its start (the last strike is over: there is no link into it now).
        /// </summary>
        private void StepPressing(float dt)
        {
            pressedFor += dt;
            Vector3 target = Target(current);
            Vector3 to = target - loco.Position;
            to.y = 0f;
            float toward = to.sqrMagnitude > 1e-4f ? Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg : loco.Facing;
            // (Turned to face the target only when it is well off to the side: less than that, the strike's own
            // turn in its wind-up brings the body round, which a turn on the spot first would only slow.)
            WantFacing = Mathf.Abs(Mathf.DeltaAngle(loco.Facing, toward)) > PressTurn ? toward : loco.Facing;
            // (Free to strike once out of the last one and standing in its guard: a strike from feet still where the
            // last one left them would have them jump to its own.)
            loco.Hurry = true;
            bool free = loco.StanceSettled;
            StrikeLink best = null;
            float bestScore = float.MaxValue, nearestMiss = float.MaxValue;
            foreach (var link in Options())
            {
                var at = Target(link.To);
                float miss = poser.MissAfterPull(link.To, at, 0f, loco.Position, loco.Facing);
                nearestMiss = Mathf.Min(nearestMiss, miss);
                float turnNeeded = Mathf.Abs(poser.AimTurnFor(link.To, at, 0f, loco.Position, loco.Facing));
                // (Facing the target first, for a while: a strike swung from a body turned away has to turn it in its wind-up.)
                if (miss <= LandWithin && (turnNeeded <= PressTurn || pressedFor > PressMost * 0.6f))
                {
                    float turn = turnNeeded;
                    float score = link.Cost + TurnWeight * turn / Mathf.Max(1f, poser.MostAimTurn);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = link;
                    }
                }
            }

            if (best != null && free)
            {
                Pressing = false;
                loco.Hurry = false;
                Want = Vector3.zero;
                Ended = "";
                poser.PlayStrike(best.To, Target(best.To));
                current = best.To;
                shotTime = 0f;
                used.Add(best.To);
                played++;
                Started?.Invoke(best.To, null);
                return;
            }

            if (pressedFor > PressMost)
            {
                Ended = $"pressed {PressMost:0.0} s after the target and no follow-on would land (the nearest would miss by {nearestMiss:0.00} m)";
                Stop();
                return;
            }

            // (Toward the target, slowing as the nearest follow-on comes within reach of it; within reach, it stops
            // and stands into its guard to strike.)
            float speed = nearestMiss <= LandWithin + StopWithin ? 0f : PressSpeed * Mathf.Sqrt(loco.Leg / 0.845f) * Mathf.Clamp01(0.3f + nearestMiss / 0.3f);
            Want = to.sqrMagnitude > 1e-4f ? to.normalized * speed : Vector3.zero;
        }

        /// <summary>One step, after the walk's (not through a hit stop).</summary>
        public void Step() => Step(1f / 60f);

        /// <summary>One step of <paramref name="dt"/> seconds, after the walk's (not through a hit stop).</summary>
        public void Step(float dt)
        {
            if (current == null)
            {
                return;
            }

            // (The strike playing follows its target, as far as its pull goes.)
            if (TargetFor != null && !Pressing)
            {
                poser.Retarget(TargetFor(current));
            }

            bool still = loco.Current == Locomotor.Mode.OneShot && loco.Shot == current;
            if (still)
            {
                // (Where the strike is, for the moment it is over: a cut-short one stops short of its links.)
                shotTime = loco.ShotTime;
            }

            if (Strikes != null)
            {
                if (Pressing)
                {
                    StepPressing(dt);
                    return;
                }

                StepChoosing(still);
                return;
            }

            if (next >= chain.Count)
            {
                if (!still)
                {
                    current = null;
                }

                return;
            }

            var link = chain[next];
            if (still && loco.ShotTime < link.At)
            {
                return;
            }

            Go(link);
            next++;
        }
    }
}
