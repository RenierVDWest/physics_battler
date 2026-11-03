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
    /// <see cref="LandWithin"/>); none does, the combo ends there rather than swing at the air.
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
            used.Clear();
            used.Add(first);
            played = 1;
            Ended = "";
            poser.PlayStrike(first, TargetFor != null ? TargetFor(first) : poser.NominalTarget());
            Started?.Invoke(first, null);
        }

        public void Stop() => current = null;

        private void Go(StrikeLink link)
        {
            loco.TurnRoot(link.Turn, Quaternion.Euler(0f, loco.Facing, 0f) * link.Shift * (loco.Leg / 0.845f));
            poser.PlayStrike(link.To, TargetFor != null ? TargetFor(link.To) : poser.NominalTarget(), startFrom: link.Entry);
            current = link.To;
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

            options.Sort((x, y) => x.Cost.CompareTo(y.Cost));
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
            StrikeLink best = null, nearest = null;
            float nearestMiss = float.MaxValue;
            foreach (var link in options)
            {
                // (From where the link leaves the root: turned and moved there.)
                Vector3 rootAt = loco.Position + Quaternion.Euler(0f, loco.Facing, 0f) * link.Shift * scale;
                float facing = loco.Facing + link.Turn;
                var target = TargetFor != null ? TargetFor(link.To) : poser.NominalTarget();
                float miss = poser.MissAfterPull(link.To, target, link.Entry, rootAt, facing);
                if (miss < nearestMiss)
                {
                    nearestMiss = miss;
                    nearest = link;
                }

                if (miss <= LandWithin)
                {
                    best = link;
                    break;
                }
            }

            if (best == null)
            {
                // (None lands from here: the strike plays on (the body moves), and at its end the combo ends.)
                if (still)
                {
                    return;
                }

                Ended = $"no follow-on would land (the nearest, {nearest?.To.Name}, would miss by {nearestMiss:0.00} m)";
                current = null;
                return;
            }

            if (still && loco.ShotTime < best.At)
            {
                return;
            }

            Go(best);
        }

        /// <summary>One step, after the walk's (not through a hit stop).</summary>
        public void Step()
        {
            if (current == null)
            {
                return;
            }

            bool still = loco.Current == Locomotor.Mode.OneShot && loco.Shot == current;
            if (Strikes != null)
            {
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
