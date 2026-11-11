using System;
using System.Collections.Generic;
using PhysicsBattler.Authority;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>A combo under way: its string, how many links it will throw, the one playing.</summary>
    public sealed class ComboRun
    {
        public ComboDefinition Definition;
        public UnitController Target;

        /// <summary>The links it means to throw (the string's, or fewer: the energy it has).</summary>
        public int Length;

        /// <summary>The link playing (0 the first).</summary>
        public int Link;

        /// <summary>Why it came out: press, counter, punish, full gather.</summary>
        public string Why;

        /// <summary>When each link started (unit time), and when each link's contact came.</summary>
        public readonly List<float> LinkStarts = new List<float>();
        public readonly List<float> Contacts = new List<float>();
    }

    /// <summary>
    /// Milestone 10: combos. One combo owns the move slot and hands it from link to link. At a link's
    /// link-out (just after contact) the next link is planned from where the body now is: the lunge
    /// is banked (the root moves to where the hips went, the feet have already stepped there), the
    /// next blow is aimed, and it starts part way into its wind-up (its link-in). For a tenth of a
    /// second the pose blends from the outgoing link's into the incoming one's, so nothing jumps.
    /// Only the last link plays its recovery. A link that doesn't reach (the target gave ground)
    /// ends the combo there, with that link's recovery; a link stopped by a shield or a body ends it,
    /// or (a string that keeps hammering) the next comes as it rebounds; a hit taken ends it.
    /// </summary>
    public partial class UnitController
    {
        /// <summary>A combo started, linked, or ended: (unit, combo, what: "started", "link 2", "ended: why").</summary>
        public static event Action<UnitController, ComboRun, string> ComboChanged;

        /// <summary>The combo under way, or null.</summary>
        public ComboRun Combo { get; private set; }
    }
}
