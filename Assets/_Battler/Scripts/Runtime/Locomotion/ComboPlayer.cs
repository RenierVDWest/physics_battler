using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// Plays a combo (the combat plan's step 5): its first strike aimed, then each next one taken up where
    /// its link has it (<see cref="StrikeLink"/>), aimed afresh, when the last one reaches its link moment,
    /// or ends early (stopped by what it met: the combo goes on from where it backed out).
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
            poser.PlayStrike(first, TargetFor != null ? TargetFor(first) : poser.NominalTarget());
            Started?.Invoke(first, null);
        }

        public void Stop() => current = null;

        /// <summary>One step, after the walk's (not through a hit stop).</summary>
        public void Step()
        {
            if (current == null)
            {
                return;
            }

            bool still = loco.Current == Locomotor.Mode.OneShot && loco.Shot == current;
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

            loco.TurnRoot(link.Turn, Quaternion.Euler(0f, loco.Facing, 0f) * link.Shift * (loco.Leg / 0.845f));
            poser.PlayStrike(link.To, TargetFor != null ? TargetFor(link.To) : poser.NominalTarget(), startFrom: link.Entry);
            current = link.To;
            next++;
            Started?.Invoke(link.To, link);
        }
    }
}
