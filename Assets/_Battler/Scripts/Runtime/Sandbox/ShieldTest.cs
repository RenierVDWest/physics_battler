using System.Collections.Generic;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>One go of the shield test: the window as measured, when the blow was started and when it landed.</summary>
    public sealed class ShieldTrial
    {
        public AimSpot Spot;
        public bool Reads;
        public float Reaction;

        /// <summary>The shield went this way, this far (metres).</summary>
        public string Way = "";
        public float Went;

        /// <summary>The part was open from, to (seconds after the uncovering began); or never, and what was still before it.</summary>
        public bool Opened;
        public float OpenedAt, ShutAt;
        public string Behind = "";

        /// <summary>What the swordsman did about it.</summary>
        public string How = "";

        /// <summary>His blow: when it was started and when it met something (seconds after the uncovering began; below 0: none), and the blow itself.</summary>
        public float StartedAt = -1f, MetAt = -1f;
        public AimBlow Blow;

        public bool Done;

        /// <summary>The blade landed on the part asked for while it was open.</summary>
        public bool Inside => Blow != null && Blow.Met && !Blow.Shield && !Blow.Parried && Blow.OnSpot && Opened && MetAt >= OpenedAt - 0.02f && MetAt <= ShutAt + 0.02f;

        /// <summary>The blade met something while the part was open.</summary>
        public bool InTime => Blow != null && Blow.Met && Opened && MetAt >= OpenedAt - 0.02f && MetAt <= ShutAt + 0.02f;

        /// <summary>How it came out, in a line.</summary>
        public string Verdict
        {
            get
            {
                string window = Opened
                    ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "the {0} was open {1:0.00} to {2:0.00} s (the shield went {3} {4:0} cm)", AimSpots.Name(Spot), OpenedAt, ShutAt, Way, 100f * Went)
                    : string.Format(System.Globalization.CultureInfo.InvariantCulture, "the {0} never came open (the shield went {1} {2:0} cm{3})", AimSpots.Name(Spot), Way, 100f * Went, Behind.Length > 0 ? "; still behind " + Behind : "");
                if (Blow == null)
                {
                    return window + "; no blow. " + How;
                }

                string when = !Opened ? "" : MetAt < 0f ? "" : MetAt < OpenedAt - 0.02f
                    ? string.Format(System.Globalization.CultureInfo.InvariantCulture, ", {0:0.00} s before it opened", OpenedAt - MetAt)
                    : MetAt > ShutAt + 0.02f
                        ? string.Format(System.Globalization.CultureInfo.InvariantCulture, ", {0:0.00} s after it shut", MetAt - ShutAt)
                        : ", inside the window";
                string met = !Blow.Met ? "met nothing" : Blow.Shield ? "met the shield" : Blow.Parried ? "was parried" : Blow.OnSpot ? "landed on the " + AimSpots.Name(Spot) : "landed on the " + Blow.Part + ", not the part asked for";
                return window + string.Format(System.Globalization.CultureInfo.InvariantCulture, "; the blow started at {0:0.00} s and {1}{2}{3}{4}. ", StartedAt, met,
                    Blow.Met ? string.Format(System.Globalization.CultureInfo.InvariantCulture, " at {0:0.00} s", MetAt) : "", when, Inside ? ": TAKEN" : "") + How;
            }
        }
    }

    /// <summary>
    /// The shield test (the plan "Fight Rhythm from Recordings", step 7; the test Renier described): the target
    /// holds a shield and uncovers a chosen part for a moment, on a button or on a loop, and the swordsman must hit
    /// that part while it is open. The swordsman takes it as an <see cref="OpeningTaker"/> does: reading the
    /// uncovering as it starts, or only reacting once he sees the part open. What is shown for each go: the window
    /// as measured on the bodies, when the blow was started and when it landed.
    /// </summary>
    public sealed class ShieldTest
    {
        /// <summary>The aim test whose swordsman and target these are: its blows are asked for and followed through it.</summary>
        public AimTest Aim;

        /// <summary>How the swordsman takes openings: reads or reacts, and his reaction time.</summary>
        public readonly OpeningTaker Taker = new OpeningTaker();

        /// <summary>The attack the swordsman takes the opening with, named by its clip (null: whichever fits the part best).</summary>
        public string Attack;

        /// <summary>The part the target uncovers and the swordsman is to hit.</summary>
        public AimSpot Spot = AimSpot.Chest;

        /// <summary>It uncovers again and again, this long after each go is over (seconds).</summary>
        public bool Loop;
        public const float Between = 1.6f;

        /// <summary>No go begins sooner than this after the last one ended (seconds): a struck target is still coming back to its guard.</summary>
        public const float Settles = 1.2f;

        /// <summary>Set (the headless run): each step of a go is written to it.</summary>
        public System.Action<string> Trace;

        /// <summary>The go under way (or the last one), and every go so far.</summary>
        public ShieldTrial Now { get; private set; }
        public readonly List<ShieldTrial> Trials = new List<ShieldTrial>();

        /// <summary>Each go as it ends.</summary>
        public event System.Action<ShieldTrial> Ended;

        /// <summary>A go is followed no longer than this (seconds).</summary>
        public const float Longest = 4f;

        private float began, endedAt = -10f, lastNow;
        private bool blowAsked;
        private string posture = "";

        public bool Running => Now != null && !Now.Done;

        /// <summary>What is going on, in a word or two, to be shown over each of the two as it happens ("": nothing): the target's tell and when the part is open; what the swordsman goes by and how it came out.</summary>
        public string TargetNote { get; private set; } = "";
        public string AttackerNote { get; private set; } = "";

        /// <summary>How long a go's outcome stays over the swordsman (seconds).</summary>
        public const float NoteStays = 1.6f;

        /// <summary>The part chosen as the swordsman sees it now (looked at ten times a second while no go is under way), and whether he has met its uncovering: for the panel.</summary>
        public string NowText { get; private set; } = "";

        private float looked = 1f;

        private void LookNow(float dt, bool open, string behind)
        {
            string known = Taker.Knows(PostureName, out float from, out float to)
                ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "he has seen this uncovering (open {0:0.00} to {1:0.00} s after it began)", from, to)
                : "he has not seen this uncovering yet";
            NowText = "its " + AimSpots.Name(Spot) + (open ? " is open" : " is covered (" + behind.Replace("its ", "") + ")") + "; " + known + ".";
        }

        // (A posture is known by what it is: the part, and its two times. Change either and it is one he has not met.)
        private string PostureName => string.Format(System.Globalization.CultureInfo.InvariantCulture, "uncover {0} {1:0.00} {2:0.00}", Spot, PostureMaker.UncoverTell, PostureMaker.UncoverFor);

        private UnitController Attacker => Aim != null ? Aim.Attacker : null;
        private UnitController Target => Aim != null ? Aim.Target : null;

        /// <summary>Begun again: the goes so far are forgotten, and what the swordsman has met.</summary>
        public void Begin(AimTest aim)
        {
            Aim = aim;
            Now = null;
            Trials.Clear();
            Taker.Forget();
            endedAt = -10f;
        }

        /// <summary>Why a go cannot begin now ("": it can).</summary>
        public string Cannot
        {
            get
            {
                if (Attacker == null || Target == null || Attacker.Clip == null || Target.Clip == null) return "there is no swordsman and target";
                if (Target.Clip.ShieldColliders.Count == 0) return "the target holds no shield";
                if (!MoveScore.On) return "movement is not made of its parts (How he moves: made)";
                if (Running) return "the last go is still under way";
                if (Aim.Following || Attacker.Clip.Busy) return "the swordsman is busy";
                if (Target.Clip.Busy || Target.Clip.IsUncovering) return "the target is busy";
                if (Target.Clip.ShieldArmHeld) return "the target's shield is still up from the last blow";
                if (!OnItsSpot(Attacker) || !OnItsSpot(Target)) return "they are not back on their spots yet";
                if (lastNow - endedAt < Settles) return "the last go has only just ended";
                if (Openings.LookAt(Target.Clip, Attacker.Clip.PartAt(PartId.UpperArmR), Spot).Open) return "its " + AimSpots.Name(Spot) + " is open already (nothing covers it to take away)";
                return "";
            }
        }

        /// <summary>The target uncovers the part now, and the swordsman takes it as he does. False: it cannot begin (<see cref="Cannot"/>).</summary>
        public bool Uncover(float now)
        {
            if (Cannot.Length > 0 || !Target.Clip.Uncover(Spot, Attacker.Clip))
            {
                return false;
            }

            began = now;
            blowAsked = false;
            if (Diagnostics.PlaytestTrace.Enabled)
            {
                Diagnostics.PlaytestTrace.Note(Target, "uncover", $"begins to uncover its {AimSpots.Name(Spot)} (the shield test); the swordsman {(Taker.Reads ? (Taker.Knows(PostureName, out _, out _) ? "reads, and has seen it" : "reads, but has not seen it yet") : "only reacts")}");
            }

            posture = PostureName;
            Now = new ShieldTrial { Spot = Spot, Reads = Taker.Reads, Reaction = Taker.Reaction };
            Trials.Add(Now);
            Taker.Began(posture, now);
            return true;
        }

        /// <summary>One step of the game, after the fighters have moved and the aim test has read its blow.</summary>
        public void Step(float dt, float now)
        {
            if (Attacker == null || Target == null || Attacker.Clip == null || Target.Clip == null)
            {
                return;
            }

            lastNow = now;
            looked += dt;
            if (looked >= 0.1f)
            {
                looked = 0f;
                var seen = Openings.LookAt(Target.Clip, Attacker.Clip.PartAt(PartId.UpperArmR), Spot);
                LookNow(dt, seen.Open, seen.Behind);
            }

            if (!Running)
            {
                TargetNote = "";
                if (now - endedAt > NoteStays)
                {
                    AttackerNote = "";
                }

                if (Loop && now - endedAt >= Between && Cannot.Length == 0)
                {
                    Uncover(now);
                }

                return;
            }

            var shown = Target.Clip.Uncovered;
            Now.Way = shown.Way;
            Now.Went = shown.Went;
            Now.Opened = shown.Opened;
            Now.OpenedAt = shown.OpenedAt;
            Now.ShutAt = shown.Shut ? shown.ShutAt : shown.OpenedAt + PostureMaker.UncoverFor;
            Now.Behind = shown.Behind ?? "";
            if (!blowAsked)
            {
                // (What he sees for himself: the part as it is from his own sword shoulder.)
                var sees = Openings.LookAt(Target.Clip, Attacker.Clip.PartAt(PartId.UpperArmR), Now.Spot);
                bool open = sees.Open;
                Trace?.Invoke(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000} s: he sees it {1}; the target's own measure: opened {2} at {3:0.00}, shut {4}; went {5:0} cm", now - began, open ? "open" : "behind " + sees.Behind, shown.Opened, shown.OpenedAt, shown.Shut, 100f * shown.Went));
                if (Taker.Step(now, open, out float landIn))
                {
                    float was = Aim.LandIn;
                    string attackWas = Aim.Attack;
                    Aim.LandIn = landIn;
                    Aim.Attack = Attack ?? attackWas;
                    blowAsked = true;
                    if (Aim.Order(Now.Spot, now))
                    {
                        Now.Blow = Aim.Now;
                        Now.StartedAt = now - began;
                    }

                    Aim.LandIn = was;
                    Aim.Attack = attackWas;
                }
            }

            Now.How = Taker.How;
            TargetNote = Target.Clip.UncoverTelling ? "TELL: his blade dips" : shown.Opened && !shown.Shut && Target.Clip.IsUncovering ? AimSpots.Name(Now.Spot).ToUpperInvariant() + " OPEN" : Target.Clip.IsUncovering ? "shield moving" : "";
            AttackerNote = Taker.Held ? "read it: holds his blow" : Now.Blow != null ? (Taker.Read ? "READ IT: striking" : "SAW IT OPEN: striking") : Taker.Read ? "read the tell" : "";
            if (Now.Blow != null && Now.Blow.Met && Now.MetAt < 0f)
            {
                Now.MetAt = Now.StartedAt + Now.Blow.MetAfter;
            }

            bool blowOver = Now.Blow == null ? blowAsked || !Target.Clip.IsUncovering && now - began > OverAfter(shown) : Now.Blow.Done;
            if ((shown.Over && blowOver) || now - began > Longest)
            {
                Now.Done = true;
                endedAt = now;
                Taker.Done();
                if (shown.Opened && shown.Shut && (Now.MetAt < 0f || Now.MetAt > shown.ShutAt))
                {
                    // (Having seen it, he has met it: its map is what was measured. Not from a go in which his own
                    // blow landed while it was open: a struck body shuts differently.)
                    Taker.Met(posture, shown.OpenedAt, shown.ShutAt);
                }

                AttackerNote = Now.Inside ? "TAKEN: on the " + AimSpots.Name(Now.Spot) + ", while open"
                    : Now.Blow == null ? (Taker.Held ? "held his blow" : "no blow")
                    : Now.Blow.Met && Now.Blow.Shield ? "TOO LATE: met the shield"
                    : Now.InTime ? "in time, but beside the part" : Now.Blow.Met ? "late, landed all the same" : "met nothing";
                TargetNote = "";
                if (Diagnostics.PlaytestTrace.Enabled)
                {
                    Diagnostics.PlaytestTrace.Note(Attacker, "shieldtest", (Now.Reads ? "reads" : "only reacts") + string.Format(System.Globalization.CultureInfo.InvariantCulture, ", reaction {0:0.00} s, tell {1:0.00} s, open for {2:0.00} s: ", Now.Reaction, PostureMaker.UncoverTell, PostureMaker.UncoverFor) + Now.Verdict);
                }

                Ended?.Invoke(Now);
            }
        }

        /// <summary>Each go begins with both where they were put, so each is taken from the same distance.</summary>
        private static bool OnItsSpot(UnitController unit)
        {
            Vector3 off = unit.Root.position - unit.DummySpot;
            off.y = 0f;
            return !unit.TargetDummy || off.magnitude <= 2f * AimTest.HeldWithin * unit.Body.SizeFactor;
        }

        // (With no blow asked, a go is over a moment after the shield is back: time for one who only reacts to have started.)
        private float OverAfter(ClipFighter.Uncovering shown) => (shown.Shut ? shown.ShutAt : 0f) + Taker.Reaction + 0.3f;

        /// <summary>The goes so far, counted.</summary>
        public string Tally()
        {
            int done = 0, taken = 0, past = 0, shield = 0, none = 0, other = 0;
            foreach (var t in Trials)
            {
                if (!t.Done) continue;
                done++;
                if (t.Inside) taken++;
                else if (t.Blow == null) none++;
                else if (t.Blow.Met && t.Blow.Shield) shield++;
                else if (t.Blow.Met && t.InTime) past++;
                else other++;
            }

            return $"{done} goes: {taken} on the part while it was open, {past} past the shield while it was open but on another part, {shield} on the shield, {other} elsewhere or on nothing, {none} with no blow";
        }
    }
}
