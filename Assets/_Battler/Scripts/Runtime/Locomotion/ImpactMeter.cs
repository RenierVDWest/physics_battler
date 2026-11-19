using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>What one body showed of an impact, read off its posed skeleton over the time the meter watches.</summary>
    public struct ImpactShown
    {
        /// <summary>The most its upper body's lean changed from how it was at contact (degrees; the line from its hips to its head).</summary>
        public float Rock;

        /// <summary>The most its shoulders turned against the way it faces (degrees): an impact off its middle twists it.</summary>
        public float Turn;

        /// <summary>The most its hips dropped (metres), its head shifted over its hips, and its shield hand moved against its chest.</summary>
        public float Drop, Head, Arm;

        /// <summary>
        /// The reaction's own share, whatever else the body was doing (a bash of its own, a cut carried through): the
        /// most its upper body was rocked by the reaction (degrees, the spine and chest together), and the most its
        /// shield arm and its sword arm were knocked back by it (metres).
        /// </summary>
        public float Reacted, ShieldArmGave, SwordArmGave;

        /// <summary>
        /// Bounce and shake (18 November, for the damping trial). <see cref="SwungBack"/>: the most the reaction's rock
        /// swung back past where it set out from, to the other side (degrees): a spring's bounce. And the body as it is
        /// seen (the physical one) against its own pose, at the head, the shield hand and the sword hand: the furthest
        /// each was knocked off its pose (metres), the furthest it then went past its pose the other way, and how many
        /// times it changed direction on the way (each change after at least a centimetre of travel).
        /// </summary>
        public float SwungBack;
        public Vector3 Off, OffPast;
        public int HeadTurns, ShieldHandTurns, SwordHandTurns;

        // (The rock at its largest; each seen part's offset at its largest, where it was last, the way it was going and how far since it turned.)
        internal Vector3 LeanPeak;
        internal Vector3[] OffPeak, OffWas;
        internal float[] OffWay, OffRun;

        // (As it was at contact.)
        internal float Turn0;
        internal Vector3 Up0, Pelvis0, Head0, Arm0, At0, AtFrom, AtTo;
        internal bool From, Sped, To;
    }

    /// <summary>
    /// One impact as the meter saw it (the plan "Impacts that carry weight", step 1): the momentum that came in along
    /// the line between the two, what was handed over through the door every impact is to go through, what was left
    /// as motion of the two bodies afterwards, and what each body showed. What is neither left as motion nor passed
    /// to the ground by the feet is unaccounted for: the force that "goes nowhere".
    /// </summary>
    public sealed class ImpactRecord
    {
        public string Kind = "";
        public ClipFighter By, On;
        public float At;

        /// <summary>From the one who dealt it to the one who took it (flat, unit).</summary>
        public Vector3 Line;

        /// <summary>The momentum that came in along the line (N·s): two bodies meeting, each one's weight times its speed along the line; a blade's blow, the blow's own.</summary>
        public float In;

        /// <summary>It is a blow of a blade (its momentum is the weapon's, not the bodies').</summary>
        public bool Blade;

        /// <summary>What the door handed the one who took it, and what it handed back to the one who dealt it (N·s). 0: this impact did not go through the door.</summary>
        public float Handed, HandedBack;

        /// <summary>Each one's ground speed along the line as they met, and afterwards (m/s; + is the way the impact goes).</summary>
        public float ByBefore, OnBefore, ByAfter, OnAfter;

        public ImpactShown ByShown, OnShown;
        public bool Done;

        /// <summary>The ledger worked the speeds out (the plan's step 3): what it left each with along the line (m/s), and what the taker's feet passed to the ground (N·s).</summary>
        public bool Worked;

        public float WorkedBy, WorkedOn, Ground;

        /// <summary>What the basher's own legs put in through the ground as he pushed on through (N·s).</summary>
        public float Drive;

        /// <summary>What is left as motion of the two bodies afterwards (N·s along the line).</summary>
        public float Left => (By != null ? By.Mass * ByAfter : 0f) + (On != null ? On.Mass * OnAfter : 0f);

        /// <summary>What came in and is not left as motion (N·s): passed to the ground, or gone.</summary>
        public float Missing => In - Left;

        public string Text
        {
            get
            {
                var c = System.Globalization.CultureInfo.InvariantCulture;
                string shown(ImpactShown s) => string.Format(c, "the reaction rocked him {5:0} degrees and swung back {8:0.0} past (shield arm back {6:0} cm, sword arm {7:0} cm); in all his upper body moved {0:0} degrees, shoulders {4:0}, hips down {1:0} cm, head {2:0} cm; as seen against his pose, head / shield hand / sword hand: knocked {9:0}/{10:0}/{11:0} cm off, back past it {12:0}/{13:0}/{14:0} cm, changed direction {15}/{16}/{17} times",
                    s.Rock, 100f * s.Drop, 100f * s.Head, 100f * s.Arm, s.Turn, s.Reacted, 100f * s.ShieldArmGave, 100f * s.SwordArmGave, s.SwungBack,
                    100f * s.Off.x, 100f * s.Off.y, 100f * s.Off.z, 100f * s.OffPast.x, 100f * s.OffPast.y, 100f * s.OffPast.z, s.HeadTurns, s.ShieldHandTurns, s.SwordHandTurns);
                string ledger = Worked ? string.Format(c, " The ledger: he leaves at {0:0.0} m/s, the other at {1:0.0}; {2:0} N·s passed to the ground by the other's feet, {4:0} N·s put in by his own legs pushing through; {3:0} N·s is in neither.", WorkedBy, WorkedOn, Ground,
                    In + Drive - (By != null ? By.Mass * WorkedBy : 0f) - (On != null ? On.Mass * WorkedOn : 0f) - Ground, Drive) : "";
                string door = Handed > 0f ? string.Format(c, " Through the door: {0:0} N·s handed over, {1:0} back.", Handed, HandedBack) : " Not through the door.";
                if (Blade)
                {
                    return string.Format(c, "{0}: the blow's {1:0} N·s.{2} After it the one struck goes {3:0.0} m/s, the striker {4:0.0}. Shown, the one struck: {5}. The striker: {6}",
                        Kind, In, door, OnAfter, ByAfter, shown(OnShown), shown(ByShown));
                }

                return string.Format(c, "{0}: {1:0} N·s came in (he at {2:0.0} m/s, the other at {3:0.0}). After it {4:0} N·s is left as motion (he {5:0.0} m/s, the other {6:0.0}); {7:0} N·s not accounted for.{8} Shown, he: {9}. The other: {10}",
                    Kind, In, ByBefore, OnBefore, Left, ByAfter, OnAfter, Missing, door + ledger, shown(ByShown), shown(OnShown));
            }
        }
    }

    /// <summary>
    /// The impact meter (the plan "Impacts that carry weight", step 1). It changes nothing: it watches the two bodies
    /// of an impact for <see cref="Watches"/> seconds and says what came in, what is left and what each showed.
    /// Every later step of that plan is judged on its line.
    /// </summary>
    public static class ImpactMeter
    {
        /// <summary>How long an impact is watched (seconds: long enough to see a spring swing back); the bodies' speed afterwards is taken from <see cref="SpeedFrom"/> to <see cref="SpeedTo"/>.</summary>
        public const float Watches = 1f, SpeedFrom = 0.25f, SpeedTo = 0.6f;

        /// <summary>Every step of every impact watched is written out (headless: FIGHT_SHAKETRACE=1).</summary>
        public static bool Trace;

        public static readonly List<ImpactRecord> Open = new List<ImpactRecord>();

        /// <summary>The last impact measured; and each as it is.</summary>
        public static ImpactRecord Last;
        public static event System.Action<ImpactRecord> Measured;

        /// <summary>An impact begins: the two, the line from the dealer to the taker, and what came in (below 0: worked out from the two bodies' speeds).</summary>
        public static ImpactRecord Begin(string kind, ClipFighter by, ClipFighter on, Vector3 line, float now, float blow = -1f)
        {
            if (by == null || on == null)
            {
                return null;
            }

            line.y = 0f;
            line = line.sqrMagnitude > 1e-8f ? line.normalized : Vector3.forward;
            var r = new ImpactRecord { Kind = kind, By = by, On = on, At = now, Line = line, Blade = blow >= 0f };
            r.ByBefore = Vector3.Dot(by.Loco.Velocity, line);
            r.OnBefore = Vector3.Dot(on.Loco.Velocity, line);
            r.In = blow >= 0f ? blow : by.Mass * r.ByBefore + on.Mass * r.OnBefore;
            by.ImpactPose(ref r.ByShown);
            on.ImpactPose(ref r.OnShown);
            // (One at a time for a pair: a new impact between the same two ends the one being watched.)
            Open.RemoveAll(o => o.By == by && o.On == on);
            if (Open.Count > 16)
            {
                Open.RemoveAt(0);
            }

            Open.Add(r);
            return r;
        }

        /// <summary>A fighter's step: what he shows of the impacts he is in.</summary>
        public static void Sample(ClipFighter who, float now)
        {
            for (int i = Open.Count - 1; i >= 0; i--)
            {
                var r = Open[i];
                if (r.By == who)
                {
                    who.ImpactWatch(ref r.ByShown, now - r.At);
                }
                else if (r.On == who)
                {
                    who.ImpactWatch(ref r.OnShown, now - r.At);
                }
                else
                {
                    continue;
                }

                if (r.ByShown.To && r.OnShown.To)
                {
                    float over = SpeedTo - SpeedFrom;
                    r.ByAfter = Vector3.Dot(r.ByShown.AtTo - r.ByShown.AtFrom, r.Line) / over;
                    r.OnAfter = Vector3.Dot(r.OnShown.AtTo - r.OnShown.AtFrom, r.Line) / over;
                    r.Done = true;
                    Open.RemoveAt(i);
                    Last = r;
                    Measured?.Invoke(r);
                    if (Diagnostics.PlaytestTrace.Enabled)
                    {
                        Diagnostics.PlaytestTrace.Note(r.By.FightUnit, "impact", r.Text);
                    }
                }
            }
        }

        public static void Clear()
        {
            Open.Clear();
            Last = null;
        }
    }
}
