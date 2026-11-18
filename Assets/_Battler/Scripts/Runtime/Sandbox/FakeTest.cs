using System.Collections.Generic;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>One fake slash of the target's, and what it left open, as measured (seconds after it began).</summary>
    public sealed class FakeGesture
    {
        public bool Long;

        /// <summary>When it began (the game's clock), when it was furthest into the attack's wind-up and how long it lasts (seconds after it began).</summary>
        public float Began, Furthest, Lasts;

        /// <summary>How many short fakes came before it since the last long one (its place in the pattern).</summary>
        public int ShortsBefore;

        /// <summary>The part was open from, to; or never.</summary>
        public bool Opened, Shut;
        public float OpenedAt, ShutAt;

        public bool Over;

        public string Text(AimSpot spot) => (Long ? "a long fake" : "a short fake")
            + string.Format(System.Globalization.CultureInfo.InvariantCulture, " (furthest at {0:0.00} s, over in {1:0.00} s): ", Furthest, Lasts)
            + (Opened ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "its {0} open {1:0.00} to {2:0.00} s, {3:0.00} s long", AimSpots.Name(spot), OpenedAt, ShutAt, ShutAt - OpenedAt) : "its " + AimSpots.Name(spot) + " never open");
    }

    /// <summary>One blow of the swordsman's in the fakes test, and how it came out.</summary>
    public sealed class FakeBlow
    {
        /// <summary>When it was started (the game's clock), and when it met something (below 0: nothing).</summary>
        public float AskedAt, MetAt = -1f;

        /// <summary>He threw it by the pattern (he foresaw the opening), not at what he saw open.</summary>
        public bool Read;

        /// <summary>The fake it was thrown at was a short one (a blow thrown by the pattern is thrown at the long one to come).</summary>
        public bool AtShort;

        /// <summary>What it met, it met while the part was open.</summary>
        public bool WhileOpen;

        public string How = "";
        public AimBlow Blow;
        public AimSpot Spot;
        public bool Done;

        /// <summary>On the part asked for, while it was open.</summary>
        public bool Taken => Blow != null && Blow.OnSpot && WhileOpen;

        public string Verdict
        {
            get
            {
                string met = Blow == null ? "no blow" : !Blow.Met ? "met nothing" : Blow.Shield ? "met the shield" : Blow.Parried ? "was parried" : Blow.OnSpot ? "landed on the " + AimSpots.Name(Spot) : "landed on the " + Blow.Part + ", not the part asked for";
                return (Read ? "thrown by the pattern" : AtShort ? "thrown at a short fake he saw open" : "thrown at a long fake he saw open")
                    + ": " + met + (Blow != null && Blow.Met ? (WhileOpen ? ", while it was open" : ", when it was covered") : "")
                    + (Taken ? ": TAKEN" : "") + ". " + How;
            }
        }
    }

    /// <summary>
    /// The fakes test (the plan "Fight Rhythm from Recordings", step 7, second part): the shield test on the real
    /// postures of step 6 in place of the scripted shield, and with the tell where Renier put it: in the pattern
    /// of a fighter's gestures. The target throws fake slashes at the swordsman (feints, made of the forehand cut
    /// as any feint is): short ones, and after so many of them a long one, on a steady beat. A fake leaves his
    /// chest open for a moment, the long one for longer. The swordsman is to hit the chest while it is open.
    ///
    /// One who only reacts goes for every chest he sees open: he falls for the short fakes, and at the long one
    /// starts too late. One who reads has watched the pattern once: he lets the short fakes go, counts them, and
    /// starts his blow before the long one begins, timed to land as it opens. Nothing of the pattern is given
    /// him: how many short ones come first, the beat, and when the long one opens the chest are what he saw.
    /// </summary>
    public sealed class FakeTest
    {
        public AimTest Aim;

        /// <summary>How the swordsman takes openings: reads or reacts, and his reaction time.</summary>
        public readonly OpeningTaker Taker = new OpeningTaker();

        /// <summary>The part he is to hit.</summary>
        public AimSpot Spot = AimSpot.Chest;

        /// <summary>How far into the attack's wind-up a short fake and a long one go (of a feint's own depth), and how long the long one is held at its furthest (seconds).</summary>
        public static float ShortDepth = 0.45f, LongDepth = 1f, LongHold = 0.25f;

        /// <summary>The pattern: so many short fakes, then a long one; a fake begins every so many seconds.</summary>
        public int Shorts = 2;
        public float Beat = 1.5f;

        /// <summary>The pattern is mixed up: the number of short fakes before each long one is drawn afresh (1 to 3), so counting them tells nothing.</summary>
        public bool MixedUp;

        /// <summary>The pattern is being played, round and round.</summary>
        public bool Pattern;

        /// <summary>The fake under way (or the last one); every fake so far; every blow so far.</summary>
        public FakeGesture Now { get; private set; }
        public readonly List<FakeGesture> Fakes = new List<FakeGesture>();
        public readonly List<FakeBlow> Blows = new List<FakeBlow>();
        public FakeBlow LastBlow => Blows.Count > 0 ? Blows[Blows.Count - 1] : null;

        /// <summary>Each fake as its opening is known, and each blow as it ends.</summary>
        public event System.Action<FakeGesture> Shown;
        public event System.Action<FakeBlow> Struck;

        /// <summary>What is going on, in a word or two, to be shown over each of the two as it happens.</summary>
        public string TargetNote { get; private set; } = "";
        public string AttackerNote { get; private set; } = "";

        /// <summary>No fake is thrown sooner than this after a blow met the target or its shield (seconds): it is still coming back to its guard.</summary>
        public const float Settles = 1.2f;

        /// <summary>The beat is lost, and the pattern begins again, when a fake comes this much late (seconds).</summary>
        public const float BeatLost = 0.35f;

        // What the swordsman has learned of the pattern, watching it: nothing of it is told him.
        private bool known;
        private int knownShorts;
        private float knownBeat, knownFrom, knownTo;

        /// <summary>What he has learned of the pattern, in words.</summary>
        public string Learned => !known ? "he has not seen the pattern through yet"
            : string.Format(System.Globalization.CultureInfo.InvariantCulture, "he has seen it: {0} short fake{1}, then the long one {2:0.00} s after the last of them begins, its chest open {3:0.00} to {4:0.00} s into it", knownShorts, knownShorts == 1 ? "" : "s", knownBeat, knownFrom, knownTo);

        private readonly List<(float from, float to)> opens = new List<(float, float)>();
        private readonly System.Random dice = new System.Random(7);
        private float nextAt = -1f, noteUntil, lastNow, lastMet = -10f;
        private int shortsSoFar, plannedShorts = -1, run;
        private float runAt, lastBegan = -1f;
        private bool struckInLong;
        private FakeBlow blow;

        private UnitController Attacker => Aim != null ? Aim.Attacker : null;
        private UnitController Target => Aim != null ? Aim.Target : null;

        public void Begin(AimTest aim)
        {
            Aim = aim;
            Now = null;
            blow = null;
            Fakes.Clear();
            Blows.Clear();
            opens.Clear();
            Pattern = false;
            Forget();
            TargetNote = AttackerNote = "";
        }

        /// <summary>He forgets the pattern.</summary>
        public void Forget()
        {
            known = false;
            Taker.Forget();
        }

        /// <summary>The pattern is begun again from its first short fake.</summary>
        public void Restart()
        {
            shortsSoFar = 0;
            plannedShorts = -1;
            lastBegan = -1f;
            nextAt = -1f;
        }

        /// <summary>Why the target cannot throw a fake now ("": it can).</summary>
        public string Cannot
        {
            get
            {
                if (Attacker == null || Target == null || Attacker.Clip == null || Target.Clip == null) return "there is no swordsman and target";
                if (Target.Clip.Strikes == null) return "the target has no sword to fake with";
                if (!AttackMaker.On) return "attacks are not made of their scores";
                if (Target.Clip.Busy || !Target.CanAct || Target.Clip.IsUncovering) return "the target is busy";
                if (Target.Clip.ShieldArmHeld) return "the target's shield is still up from the last blow";
                if (!OnItsSpot(Target)) return "the target is not back on its spot yet";
                if (lastNow - lastMet < Settles) return "the target has only just been struck";
                return "";
            }
        }

        private static bool OnItsSpot(UnitController unit)
        {
            Vector3 off = unit.Root.position - unit.DummySpot;
            off.y = 0f;
            return !unit.TargetDummy || off.magnitude <= 2f * AimTest.HeldWithin * unit.Body.SizeFactor;
        }

        /// <summary>The target throws one fake now, short or long. False: it cannot (<see cref="Cannot"/>).</summary>
        public bool Throw(bool isLong, float now)
        {
            if (Cannot.Length > 0)
            {
                return false;
            }

            // (A feint made of the forehand cut at the swordsman's chest, as any feint is; only how deep it goes and how
            // long it is held differ between the short and the long.)
            float holdWas = PostureMaker.FeintHold;
            PostureMaker.FeintHold = isLong ? LongHold : 0f;
            PostureMaker.Ask(isLong ? LongDepth : ShortDepth, 0f);
            // (Cut short of a real attack: the short fake is cut at the near end of the range, the long one at the far end and held.)
            PostureMaker.NextCutAt = PostureMaker.CutShort ? Mathf.Clamp(PostureMaker.CutAt + (isLong ? 1f : -1f) * PostureMaker.CutVaries, 0.05f, 0.95f) : -1f;
            PostureMaker.NextCutHold = PostureMaker.CutShort && isLong ? LongHold : -1f;
            bool thrown = PostureMaker.CutShort
                ? Target.AimStrike(Attacker, AimSpot.Chest, out _, out _, null, null, null, 0f, null, PostureMaker.NextFakeClass())
                : Target.AimStrike(Attacker, AimSpot.Chest, out _, out _, null, null, "sns_forehand");
            PostureMaker.FeintHold = holdWas;
            PostureMaker.NextCutAt = PostureMaker.NextCutHold = -1f;
            var made = thrown ? Target.Clip.Made : null;
            if (made == null || !made.Posture)
            {
                PostureMaker.Take(out _, out _);
                return false;
            }

            float pace = Mathf.Max(0.05f, Target.Clip.Loco.TimeScale);
            Finish(now - (Now != null ? Now.Began : now));
            Now = new FakeGesture { Long = isLong, Began = now, Furthest = made.ContactAt / pace, Lasts = made.Clip.Duration / pace, ShortsBefore = shortsSoFar };
            Fakes.Add(Now);
            run = 0;
            struckInLong = false;
            Decide(now);
            return true;
        }

        /// <summary>What the swordsman makes of the fake just begun.</summary>
        private void Decide(float now)
        {
            if (Taker.Pending)
            {
                // (He has a blow planned by the pattern: this fake changes nothing.)
                return;
            }

            if (!Taker.Reads)
            {
                Taker.Watch(now, "he does not read: he goes for what he sees open");
                Note("", now);
                return;
            }

            Taker.Done();
            if (!known)
            {
                Note(Pattern ? "watching: learning his pattern" : "reads: nothing to go by", now);
                return;
            }

            int count = Now.ShortsBefore + (Now.Long ? 0 : 1);
            if (Pattern && !Now.Long && count == knownShorts)
            {
                // (The last short one: the long one comes a beat after it began, and he knows when that opens the chest.)
                float begins = now + knownBeat;
                bool goes = Taker.Foresee(now, begins + knownFrom, begins + knownTo, string.Format(System.Globalization.CultureInfo.InvariantCulture, "the pattern ({0} short fake{1}, then the long one)", knownShorts, knownShorts == 1 ? "" : "s"));
                Note(goes ? "READ THE PATTERN: the long one is next" : "read the pattern: holds his blow", now);
            }
            else
            {
                Note("reads: lets the " + (Now.Long ? "long" : "short") + " fake go", now);
            }
        }

        private void Note(string note, float now, float stays = 1.2f)
        {
            AttackerNote = note;
            noteUntil = now + stays;
        }

        /// <summary>One step of the game, after the fighters have moved and the aim test has read its blow.</summary>
        public void Step(float dt, float now)
        {
            lastNow = now;
            if (Attacker == null || Target == null || Attacker.Clip == null || Target.Clip == null)
            {
                return;
            }

            if (now > noteUntil && (blow == null || blow.Done) && !Taker.Pending)
            {
                AttackerNote = "";
            }

            bool open = Openings.LookAt(Target.Clip, Attacker.Clip.PartAt(PartId.UpperArmR), Spot).Open;
            StepFake(now, open);
            StepBlow(now, open);
            StepPattern(now);
        }

        /// <summary>The fake under way: when it has the part open, measured as the shield test measures it.</summary>
        private void StepFake(float now, bool open)
        {
            TargetNote = "";
            if (Now == null || Now.Over)
            {
                return;
            }

            float t = now - Now.Began;
            run = open == (run > 0) ? run + (open ? 1 : -1) : open ? 1 : -1;
            if (run == 1 || run == -1)
            {
                runAt = t;
            }

            if (run >= 3 && !Now.Opened)
            {
                Now.Opened = true;
                Now.OpenedAt = runAt;
            }
            else if (run <= -3 && Now.Opened && !Now.Shut)
            {
                Now.Shut = true;
                Now.ShutAt = runAt;
            }

            TargetNote = (Now.Long ? "LONG FAKE" : "short fake") + (Now.Opened && !Now.Shut ? ": " + AimSpots.Name(Spot).ToUpperInvariant() + " OPEN" : "");
            // (Over once it is shut again past its furthest, or as its clip ends.)
            if ((Now.Shut && t > Now.Furthest) || t > Now.Lasts + 0.1f)
            {
                Finish(t);
            }
        }

        /// <summary>The fake under way is over: what it opened is kept, learned from and told.</summary>
        private void Finish(float t)
        {
            if (Now == null || Now.Over)
            {
                return;
            }

            if (Now.Opened && !Now.Shut)
            {
                Now.Shut = true;
                Now.ShutAt = t;
            }

            Now.Over = true;
            TargetNote = "";
            if (Now.Opened)
            {
                opens.Add((Now.Began + Now.OpenedAt, Now.Began + Now.ShutAt));
            }

            Learn();
            if (Diagnostics.PlaytestTrace.Enabled)
            {
                Diagnostics.PlaytestTrace.Note(Target, "fake", Now.Text(Spot));
            }

            Shown?.Invoke(Now);
        }

        /// <summary>Having watched a long fake come after its short ones on the beat, he knows the pattern: what he saw is what he goes by.</summary>
        private void Learn()
        {
            if (!Pattern || !Now.Long || !Now.Opened || Now.ShortsBefore < 1 || lastBeat <= 0f || struckInLong)
            {
                return;
            }

            known = true;
            knownShorts = Now.ShortsBefore;
            knownBeat = lastBeat;
            knownFrom = Now.OpenedAt;
            knownTo = Now.ShutAt;
        }

        private float lastBeat;

        private void StepBlow(float now, bool open)
        {
            if (blow != null && !blow.Done)
            {
                if (Now != null && Now.Long && !Now.Over)
                {
                    // (A long fake he struck into is not one to learn from: a struck body shuts differently.)
                    struckInLong = true;
                }

                if (blow.Blow.Met && blow.MetAt < 0f)
                {
                    blow.MetAt = blow.AskedAt + blow.Blow.MetAfter;
                    lastMet = now;
                    // (Open as it met: in a window already closed and measured, or in the one of the fake under way.)
                    foreach (var (from, to) in opens)
                    {
                        blow.WhileOpen |= blow.MetAt >= from - 0.02f && blow.MetAt <= to + 0.02f;
                    }

                    if (Now != null && !Now.Over && Now.Opened && !Now.Shut)
                    {
                        blow.WhileOpen = true;
                    }

                    if (Now != null && Now.Long && !Now.Over)
                    {
                        struckInLong = true;
                    }
                }

                if (blow.Blow.Done)
                {
                    blow.Done = true;
                    Note(blow.Taken ? "TAKEN: on the " + AimSpots.Name(Spot) + ", while open"
                        : blow.Blow.Met && blow.Blow.Shield ? (blow.Read ? "MISREAD THE PATTERN: met the shield" : blow.AtShort ? "FELL FOR A FAKE: met the shield" : "TOO LATE: met the shield")
                        : blow.WhileOpen && blow.Blow.Met ? "in time, but beside the part" : blow.Blow.Met ? "landed when it was covered" : "met nothing", now, 1.6f);
                    if (Diagnostics.PlaytestTrace.Enabled)
                    {
                        Diagnostics.PlaytestTrace.Note(Attacker, "fakes", (Taker.Reads ? "reads" : "only reacts") + string.Format(System.Globalization.CultureInfo.InvariantCulture, ", reaction {0:0.00} s: ", Taker.Reaction) + blow.Verdict);
                    }

                    Struck?.Invoke(blow);
                }

                return;
            }

            if (!Taker.Step(now, open, out float landIn))
            {
                return;
            }

            if (Aim.Following || Attacker.Clip.Busy)
            {
                Note("he was still in his last blow", now);
                return;
            }

            float was = Aim.LandIn;
            Aim.LandIn = landIn;
            bool asked = Aim.Order(Spot, now);
            Aim.LandIn = was;
            if (!asked)
            {
                return;
            }

            blow = new FakeBlow { AskedAt = now, Read = Taker.Read, AtShort = !Taker.Read && Now != null && !Now.Long, How = Taker.How, Blow = Aim.Now, Spot = Spot };
            Blows.Add(blow);
            Note(Taker.Read ? "READ THE PATTERN: striking" : "SAW IT OPEN: striking", now, 3f);
        }

        /// <summary>The pattern: a fake on each beat, so many short ones and then a long one. A beat lost (the target struck, or off its spot) begins it again.</summary>
        private void StepPattern(float now)
        {
            if (!Pattern)
            {
                nextAt = -1f;
                return;
            }

            if (nextAt < 0f)
            {
                nextAt = now;
                shortsSoFar = 0;
                plannedShorts = -1;
                lastBegan = -1f;
            }

            if (now < nextAt || Cannot.Length > 0)
            {
                return;
            }

            if (now - nextAt > BeatLost)
            {
                shortsSoFar = 0;
                plannedShorts = -1;
                lastBegan = -1f;
                Taker.Done();
            }

            if (plannedShorts < 0)
            {
                plannedShorts = MixedUp ? 1 + dice.Next(3) : Mathf.Max(1, Shorts);
            }

            bool isLong = shortsSoFar >= plannedShorts;
            float before = lastBegan;
            if (!Throw(isLong, now))
            {
                return;
            }

            lastBeat = before >= 0f ? now - before : 0f;
            lastBegan = now;
            nextAt = now + Beat;
            if (isLong)
            {
                shortsSoFar = 0;
                plannedShorts = -1;
            }
            else
            {
                shortsSoFar++;
            }
        }

        /// <summary>The fakes and the blows so far, counted.</summary>
        public string Tally()
        {
            int longs = 0, shorts = 0, taken = 0, beside = 0, fell = 0, late = 0, misread = 0, nothing = 0, done = 0;
            foreach (var f in Fakes)
            {
                if (f.Long) longs++;
                else shorts++;
            }

            foreach (var b in Blows)
            {
                if (!b.Done) continue;
                done++;
                if (b.Taken) taken++;
                else if (b.WhileOpen && b.Blow.Met && !b.Blow.Shield) beside++;
                else if (!b.Blow.Met) nothing++;
                else if (b.Read) misread++;
                else if (b.AtShort) fell++;
                else late++;
            }

            return $"{longs} long fakes and {shorts} short ones; {done} blows: {taken} on the part while it was open, {beside} in time but beside the part, {misread} thrown by the pattern when it was not open, {fell} wasted on a short fake, {late} too late at a long one, {nothing} that met nothing";
        }
    }
}
