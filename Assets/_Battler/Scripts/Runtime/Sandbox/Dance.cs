using System.Collections.Generic;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>The gestures a fighter throws between blows (the postures of the plan's step 6).</summary>
    public enum Gesture
    {
        /// <summary>A fake slash, turned back early; one that goes all the way into the wind-up and is held; a fake lunge.</summary>
        ShortFake, LongFake, FakeLunge,

        /// <summary>The blade's bluffs and the shield's press; a drop of the hips.</summary>
        BladeDip, BladeFlick, BladeCircle, ShieldPress, Drop,
    }

    /// <summary>
    /// The pattern a fighter's gestures come in (the plan "Fight Rhythm from Recordings", step 8, the dance, its
    /// second part): so many gestures on a beat, then the one that comes before his real blow (his tell), then
    /// the blow. It is his own: which gestures, how many, which is the tell, the beat. A poorer swordsman's is
    /// plain (always the same); a better one's is mixed up (the number changes, and the tell sometimes comes with
    /// no blow after it).
    /// </summary>
    public sealed class GesturePattern
    {
        public string Name = "Plain";

        /// <summary>The gestures thrown before the tell, in turn, round and round.</summary>
        public Gesture[] Fillers = { Gesture.ShortFake };

        /// <summary>How many of them come before the tell.</summary>
        public int Count = 2;

        /// <summary>The gesture that comes before his real blow.</summary>
        public Gesture Tell = Gesture.LongFake;

        /// <summary>From the start of one gesture to the start of the next (seconds); from the start of the tell to the start of the blow.</summary>
        public float Beat = 1.5f, BlowAfter = 1.1f;

        /// <summary>Mixed up: the number of gestures before the tell is drawn afresh each time (1 to 3), and one tell in three has no blow after it.</summary>
        public bool MixedUp;

        public string Text
        {
            get
            {
                var kinds = new List<string>();
                foreach (var g in Fillers)
                {
                    kinds.Add(Dance.Name(g));
                }

                return (MixedUp ? "one to three of " : Count + " of ") + string.Join(" / ", kinds) + ", then " + Dance.Name(Tell) + ", then his blow"
                    + string.Format(System.Globalization.CultureInfo.InvariantCulture, "; a gesture every {0:0.0} s", Beat) + (MixedUp ? "; mixed up" : "; always the same");
            }
        }

        /// <summary>The pattern that goes with one of the made-up fighters (<see cref="FighterLevers"/>): tries, not styles decided on.</summary>
        public static GesturePattern Named(string name)
        {
            switch ((name ?? "").Trim().ToLowerInvariant())
            {
                case "prowler":
                    // (Quick small feelers, then he sinks before he springs.)
                    return new GesturePattern { Name = "Prowler", Fillers = new[] { Gesture.BladeFlick, Gesture.ShortFake }, Count = 3, Tell = Gesture.Drop, Beat = 1.2f, BlowAfter = 0.45f };
                case "strutter":
                    // (Showy: blade circles and short fakes, and a long fake slash before the real one.)
                    return new GesturePattern { Name = "Strutter", Fillers = new[] { Gesture.BladeCircle, Gesture.ShortFake }, Count = 2, Tell = Gesture.LongFake, Beat = 1.6f, BlowAfter = 1.2f };
                case "bull":
                    // (Few frills: he presses with his shield, and his point dips before he cuts.)
                    return new GesturePattern { Name = "Bull", Fillers = new[] { Gesture.ShieldPress }, Count = 2, Tell = Gesture.BladeDip, Beat = 1.9f, BlowAfter = 0.3f };
                default:
                    return new GesturePattern();
            }
        }
    }

    /// <summary>One gesture of a fighter's, and what it left open, as the other measured it (seconds after it began).</summary>
    public sealed class DanceGesture
    {
        public Gesture Kind;
        public bool Tell;

        /// <summary>Its place among the gestures since his pattern began (0: the first).</summary>
        public int Place;

        /// <summary>When it began (the game's clock), when it is furthest out and how long it lasts (seconds after it began).</summary>
        public float Began, Furthest, Lasts;

        /// <summary>His chest was open from, to; or never.</summary>
        public bool Opened, Shut;
        public float OpenedAt, ShutAt;

        /// <summary>It is over; a blow met him in the middle of it.</summary>
        public bool Over, StruckIn;

        /// <summary>The other has made out what it is (a fake slash is not known for short or long as it starts).</summary>
        public bool MadeOut;

        /// <summary>It commits his shield (a fake of his own: <see cref="ClipFighter.PostureCommitsShield"/>).</summary>
        public bool Commits => Kind == Gesture.ShortFake || Kind == Gesture.LongFake || Kind == Gesture.FakeLunge;

        public string Text => Dance.Name(Kind) + (Tell ? " (his tell)" : "")
            + (Opened ? string.Format(System.Globalization.CultureInfo.InvariantCulture, ": chest open {0:0.00} to {1:0.00} s", OpenedAt, ShutAt) : ": chest never open");
    }

    /// <summary>One real blow in the dance, and how it came out.</summary>
    public sealed class DanceBlow
    {
        public Dancer By;
        public float AskedAt, MetAt = -1f;

        /// <summary>Why he threw it: "pattern" (it is his pattern's blow), "read" (he foresaw an opening), "saw" (he saw the chest open).</summary>
        public string Why = "";
        public string How = "";
        public bool Met, Shield, Parried, Done;
        public PartId Part;

        /// <summary>As it met: the other was in the middle of a fake of his own (his shield committed); the other was covering (he expected it).</summary>
        public bool CaughtInFake, Expected;

        public bool Landed => Met && !Shield && !Parried;

        public string Verdict => (Why == "pattern" ? "his pattern's blow" : Why == "read" ? "thrown by what he has read" : "thrown at a chest he saw open")
            + ": " + (!Met ? "met nothing" : Shield ? "met the shield" : Parried ? "was parried" : "LANDED on the " + Part)
            + (Met && CaughtInFake ? " (the other was in a fake of his own)" : "") + (Met && Expected ? " (the other expected it and was covering)" : "")
            + (How.Length > 0 ? ". " + How : "");
    }

    /// <summary>
    /// One of the two in the dance: his own pattern of gestures, what he has learned of the other's by watching,
    /// and what he does with it.
    ///
    /// What he does with it, if he reads: knowing the other's tell, he stops gesturing when he sees it and keeps
    /// his shield free until the blow has come (a fake of his own would commit his shield: the blow would land);
    /// knowing which of the other's gestures leaves the chest open and when it comes, he starts a blow of his own
    /// early, timed to land in the opening. One who does not read gestures on regardless, and goes for a chest
    /// when he sees it open.
    /// </summary>
    public sealed class Dancer
    {
        public UnitController Unit;
        public Dancer Foe;
        public GesturePattern Pattern = new GesturePattern();

        /// <summary>He reads the other's pattern (set by the lab; <see cref="Dance.Reading"/> off, nobody does).</summary>
        public bool Reads = true;

        /// <summary>How he takes openings, and his reaction time (<see cref="OpeningTaker"/>).</summary>
        public readonly OpeningTaker Taker = new OpeningTaker();

        /// <summary>His gesture under way (or his last); all of them; his blows.</summary>
        public DanceGesture Now;
        public readonly List<DanceGesture> Gestures = new List<DanceGesture>();
        public readonly List<DanceBlow> Blows = new List<DanceBlow>();

        /// <summary>What is going on with him, in a word or two, to be shown over him.</summary>
        public string Note = "";
        internal float noteUntil;

        // ---- His own pattern, as it goes ----
        internal float nextAt = -1f, blowAt = -1f, lastMet = -10f, restUntil;
        internal int place, planned = -1;
        internal bool tellThrown;
        internal DanceBlow blow;

        // ---- What he has learned of the other's, by watching: nothing of it is given him ----
        internal readonly List<(Gesture kind, float began)> seen = new List<(Gesture, float)>();
        internal readonly List<(Gesture tell, float delay, int count, float beat, bool clean)> cycles = new List<(Gesture, float, int, float, bool)>();
        internal bool knowsTell, knowsCount;
        internal Gesture knownTell;
        internal float knownDelay, knownBeat;
        internal int knownCount;

        // ---- What he expects ----
        internal float coverFrom = -1f, coverUntil = -1f, expectedAt, pausedAt = -1f;
        internal bool expectedCame, coverByTell;
        internal int openRun;
        internal float openRunAt;

        /// <summary>He expects a blow and keeps his shield free: no gesture, no blow of his pattern.</summary>
        public bool Covering(float now) => coverFrom >= 0f && now >= coverFrom && now <= coverUntil;

        /// <summary>He is covering because he made out the other's tell (not only because he saw the blow start).</summary>
        public bool Expecting(float now) => Covering(now) && coverByTell;

        /// <summary>Blows of his that landed on the other's body, that met its shield or sword, that met nothing; blows of the other's that landed on him.</summary>
        public int Landed, Stopped, Missed, Taken;

        /// <summary>Of the blows that landed on him: how many while he was in a fake of his own; how many of the other's blows he had expected.</summary>
        public int CaughtInFake, ExpectedBlows, CoveredForNothing, ReadBlows;

        /// <summary>How the fight is going for him: blows landed less blows taken, over the last <see cref="Dance.GoingOver"/> seconds.</summary>
        public int Going(float now)
        {
            int going = 0;
            foreach (float at in landedAt)
            {
                going += now - at <= Dance.GoingOver ? 1 : 0;
            }

            foreach (float at in Foe.landedAt)
            {
                going -= now - at <= Dance.GoingOver ? 1 : 0;
            }

            return going;
        }

        internal readonly List<float> landedAt = new List<float>();

        /// <summary>What he has learned of the other's pattern, in words.</summary>
        public string Learned
        {
            get
            {
                if (!Taker.Reads)
                {
                    return "he does not read";
                }

                string tell = knowsTell ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "the blow comes {0:0.00} s after {1} begins", knownDelay, Dance.Name(knownTell)) : "he does not know the tell yet";
                string count = knowsCount ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "; {0} gesture{1} come before it, {2:0.0} s apart", knownCount, knownCount == 1 ? "" : "s", knownBeat) : knowsTell ? "; he cannot count them" : "";
                string open = knowsTell && Taker.Knows(Dance.Name(knownTell), out float from, out float to) ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "; it leaves the chest open {0:0.00} to {1:0.00} s", from, to) : knowsTell ? "; it leaves no opening" : "";
                return tell + count + open;
            }
        }

        internal void Forget()
        {
            seen.Clear();
            cycles.Clear();
            knowsTell = knowsCount = false;
            Taker.Forget();
            Taker.Done();
            coverFrom = coverUntil = -1f;
        }

        internal void Say(string note, float now, float stays = 1.3f)
        {
            Note = note;
            noteUntil = now + stays;
        }
    }

    /// <summary>
    /// The dance's second part (the plan "Fight Rhythm from Recordings", step 8): two fighters stand in reach of
    /// each other, each gesturing in a pattern of his own and throwing his real blow after his tell, and each
    /// watching the other. The shield test and the fakes test had one who gestured and one who struck; here both
    /// do both.
    ///
    /// What decides a blow is what was already in the game: a fake of his own commits a fighter's shield, so a
    /// blow that comes while he is in one lands; out of one, his shield goes to meet it. A fighter who has read
    /// the other's tell stops gesturing when he sees it; one who has not gestures on, and is caught when the
    /// blow happens to come in the middle of a fake. And a gesture that leaves the chest open for long enough is
    /// struck into by one who knows it is coming.
    ///
    /// "How the fight is going" is first kept here: blows landed less blows taken, over the last seconds.
    /// </summary>
    public sealed class Dance
    {
        /// <summary>Fighters read each other's patterns (Combat Feel "Fighters Read Each Other's Patterns"). Off: none of them reads, whatever he is set to.</summary>
        public static bool Reading = true;

        public readonly Dancer A = new Dancer(), B = new Dancer();

        /// <summary>The two are dancing.</summary>
        public bool On;

        /// <summary>No gesture sooner than this after a blow met him or his shield (seconds); the beat is lost when a gesture comes this much late.</summary>
        public const float Settles = 1.2f, BeatLost = 0.4f;

        /// <summary>How the fight is going is counted over this long (seconds).</summary>
        public const float GoingOver = 20f;

        /// <summary>A tell is known once the same gesture has come before this many of the other's blows running.</summary>
        public const int LearnsAfter = 2;

        /// <summary>How far into the wind-up a short fake and a long one go (of a feint's own depth), and how long the long one is held (the fakes test's own).</summary>
        public static float ShortDepth => FakeTest.ShortDepth;
        public static float LongDepth => FakeTest.LongDepth;
        public static float LongHold => FakeTest.LongHold;

        public event System.Action<Dancer, DanceGesture> Shown;
        public event System.Action<DanceBlow> Struck;
        public event System.Action<Dancer, string> Told;

        private readonly System.Random dice = new System.Random(11);
        private float lastNow;

        public static string Name(Gesture g)
        {
            switch (g)
            {
                case Gesture.ShortFake: return "a short fake";
                case Gesture.LongFake: return "a long fake";
                case Gesture.FakeLunge: return "a fake lunge";
                case Gesture.BladeDip: return "a blade dip";
                case Gesture.BladeFlick: return "a blade flick";
                case Gesture.BladeCircle: return "a blade circle";
                case Gesture.ShieldPress: return "a shield press";
                default: return "a drop";
            }
        }

        public void Begin(UnitController a, UnitController b)
        {
            A.Unit = a;
            B.Unit = b;
            A.Foe = B;
            B.Foe = A;
            On = false;
            foreach (var d in new[] { A, B })
            {
                d.Now = null;
                d.Gestures.Clear();
                d.Blows.Clear();
                d.blow = null;
                d.Note = "";
                d.Landed = d.Stopped = d.Missed = d.Taken = d.CaughtInFake = d.ExpectedBlows = d.CoveredForNothing = d.ReadBlows = 0;
                d.landedAt.Clear();
                d.Forget();
                Restart(d, -1f);
            }
        }

        /// <summary>What each has learned of the other is forgotten.</summary>
        public void Forget()
        {
            A.Forget();
            B.Forget();
        }

        private static void Restart(Dancer d, float at)
        {
            d.place = 0;
            d.planned = -1;
            d.tellThrown = false;
            d.blowAt = -1f;
            d.nextAt = at;
        }

        public bool Ready => A.Unit != null && B.Unit != null && A.Unit.Clip != null && B.Unit.Clip != null;

        /// <summary>Why the dance cannot be danced ("": it can).</summary>
        public string Cannot
        {
            get
            {
                if (!Ready) return "there are no two fighters";
                if (A.Unit.Clip.Strikes == null || B.Unit.Clip.Strikes == null) return "both need a sword";
                if (!AttackMaker.On) return "attacks are not made of their scores";
                return "";
            }
        }

        private static bool OnItsSpot(UnitController unit)
        {
            Vector3 off = unit.Root.position - unit.DummySpot;
            off.y = 0f;
            return !unit.TargetDummy || off.magnitude <= 2f * AimTest.HeldWithin * unit.Body.SizeFactor;
        }

        private bool Free(Dancer d, float now)
        {
            var clip = d.Unit.Clip;
            return !clip.Busy && d.Unit.CanAct && !clip.IsUncovering && !clip.ShieldArmHeld && OnItsSpot(d.Unit) && now - d.lastMet >= Settles && now >= d.restUntil;
        }

        // ---- A hit of the fight's ------------------------------------------------------------------------------------

        /// <summary>A hit of the fight's: what a blow of one of the two met.</summary>
        public void OnHit(HitReport hit, float now)
        {
            if (hit == null || !On || !Ready)
            {
                return;
            }

            var by = hit.Attacker == A.Unit ? A : hit.Attacker == B.Unit ? B : null;
            if (by == null || hit.Struck != by.Foe.Unit)
            {
                return;
            }

            var foe = by.Foe;
            foe.lastMet = now;
            if (foe.Now != null && !foe.Now.Over)
            {
                foe.Now.StruckIn = true;
            }

            var blow = by.blow;
            if (blow == null || blow.Done || blow.Met)
            {
                return;
            }

            blow.Met = true;
            blow.MetAt = now;
            blow.Shield = hit.Shield;
            blow.Parried = hit.Parried;
            blow.Part = hit.Part;
            blow.CaughtInFake = foe.Now != null && !foe.Now.Over && foe.Now.Commits;
            blow.Expected = foe.Expecting(now);
        }

        // ---- One step ------------------------------------------------------------------------------------------------

        /// <summary>One step of the game, after the fighters have moved.</summary>
        public void Step(float dt, float now)
        {
            lastNow = now;
            if (!Ready)
            {
                return;
            }

            foreach (var d in new[] { A, B })
            {
                d.Taker.Reads = Reading && d.Reads;
                if (now > d.noteUntil)
                {
                    d.Note = "";
                }
            }

            if (!On)
            {
                return;
            }

            // (What each leaves open as the other sees it, measured on the bodies as the tests measure it.)
            bool openA = Openings.LookAt(A.Unit.Clip, B.Unit.Clip.PartAt(PartId.UpperArmR), AimSpot.Chest).Open;
            bool openB = Openings.LookAt(B.Unit.Clip, A.Unit.Clip.PartAt(PartId.UpperArmR), AimSpot.Chest).Open;
            StepGesture(A, now, openA);
            StepGesture(B, now, openB);
            StepBlow(A, now, openB);
            StepBlow(B, now, openA);
            StepPattern(A, now);
            StepPattern(B, now);
        }

        /// <summary>His gesture under way: what it leaves open, and when the other makes out what it is.</summary>
        private void StepGesture(Dancer d, float now, bool open)
        {
            var g = d.Now;
            if (g == null || g.Over)
            {
                return;
            }

            float t = now - g.Began;
            d.openRun = open == (d.openRun > 0) ? d.openRun + (open ? 1 : -1) : open ? 1 : -1;
            if (d.openRun == 1 || d.openRun == -1)
            {
                d.openRunAt = t;
            }

            if (d.openRun >= 3 && !g.Opened)
            {
                g.Opened = true;
                g.OpenedAt = d.openRunAt;
            }
            else if (d.openRun <= -3 && g.Opened && !g.Shut)
            {
                g.Shut = true;
                g.ShutAt = d.openRunAt;
            }
            else if (d.openRun >= 3 && g.Shut)
            {
                // (Open again within the same gesture: one opening, from the first of it to the last.)
                g.Shut = false;
            }

            // (A fake slash starts as a short one, a long one and a real cut all start: it is a long one once it has
            // gone past where a short one turns back.)
            if (!g.MadeOut && g.Kind == Gesture.LongFake && t >= ShortTurnsAt(g))
            {
                g.MadeOut = true;
                MadeOut(d.Foe, g, now);
            }

            // (Over when its motion is: until then he is in it, and a fake's shield is committed.)
            if (t > g.Lasts + 0.05f)
            {
                Finish(d, t);
            }
        }

        /// <summary>When a long fake has gone past where a short one would have turned back (seconds after it began).</summary>
        private static float ShortTurnsAt(DanceGesture g) => g.Furthest * Mathf.Clamp01(ShortDepth / Mathf.Max(0.05f, LongDepth)) + 0.08f;

        private void Finish(Dancer d, float t)
        {
            var g = d.Now;
            if (g == null || g.Over)
            {
                return;
            }

            if (g.Opened && !g.Shut)
            {
                g.Shut = true;
                g.ShutAt = t;
            }

            g.Over = true;
            // (What it left open is what the other saw of it: the next of its kind he knows. Not one he struck into: a struck body shuts differently.)
            if (!g.StruckIn)
            {
                if (g.Opened)
                {
                    d.Foe.Taker.Met(Name(g.Kind), g.OpenedAt, g.ShutAt);
                }
                else if (d.Foe.Taker.Knows(Name(g.Kind), out _, out _))
                {
                    d.Foe.Taker.Met(Name(g.Kind), 0f, 0f);
                }
            }

            Shown?.Invoke(d, g);
        }

        /// <summary>The other begins a gesture: what a fighter makes of it as it starts.</summary>
        private void Sees(Dancer d, DanceGesture g, float now)
        {
            if (d.coverFrom >= 0f && !d.expectedCame)
            {
                // (He was waiting for a blow after a tell, and the other gestures again: no blow is coming.)
                d.CoveredForNothing++;
                d.Say("covered for nothing: no blow came", now);
                d.coverFrom = d.coverUntil = -1f;
                d.seen.Clear();
            }
            else if (d.seen.Count > 0 && g.Began - d.seen[d.seen.Count - 1].began > (d.knowsCount ? d.knownBeat + BeatLost - 0.05f : 3.5f))
            {
                // (A gap in his gestures: his pattern has begun again.)
                d.seen.Clear();
            }

            d.seen.Add((g.Kind, g.Began));
            if (!d.Taker.Reads)
            {
                if (!d.Taker.Pending && (d.blow == null || d.blow.Done))
                {
                    d.Taker.Watch(now, "he does not read: he goes for what he sees open");
                }

                return;
            }

            // (By the count: this is the last gesture before the tell, and the tell leaves an opening he knows.)
            int since = SinceBlow(d);
            if (d.knowsCount && d.knowsTell && since == d.knownCount && !d.Taker.Pending && (d.blow == null || d.blow.Done)
                && d.Taker.Knows(Name(d.knownTell), out float from, out float to) && to - from > 0.05f)
            {
                float begins = now + d.knownBeat;
                bool goes = d.Taker.Foresee(now, begins + from, begins + to, "the count (" + d.knownCount + " gestures, then " + Name(d.knownTell) + ")");
                d.Say(goes ? "READ THE COUNT: his tell is next" : "read the count: holds his blow", now, 2f);
                Told?.Invoke(d, d.Taker.How);
            }

            // (By the count, too: this one is the tell itself, whatever it looks like as it starts.)
            bool byCount = d.knowsCount && d.knowsTell && since == d.knownCount + 1;
            if (g.Kind != Gesture.LongFake || byCount)
            {
                g.MadeOut = true;
                MadeOut(d, g, now, byCount && g.Kind == Gesture.LongFake);
            }
        }

        /// <summary>How many gestures of the other's he has seen since its last blow (this one counted).</summary>
        private static int SinceBlow(Dancer d) => d.seen.Count;

        /// <summary>He has made out what the other's gesture is.</summary>
        private void MadeOut(Dancer d, DanceGesture g, float now, bool byCount = false)
        {
            if (!d.Taker.Reads)
            {
                return;
            }

            if (d.knowsTell && g.Kind == d.knownTell)
            {
                // (His tell: the blow comes so long after it began. Until it has come, no fake of his own.)
                float expected = g.Began + d.knownDelay;
                d.coverFrom = now + d.Taker.Reaction;
                d.coverUntil = expected + 1.4f;
                d.expectedAt = expected;
                d.expectedCame = false;
                d.coverByTell = true;
                d.Say(byCount ? "READ THE COUNT: covers for his blow" : "SAW HIS TELL: covers for his blow", now, 2f);
                Told?.Invoke(d, string.Format(System.Globalization.CultureInfo.InvariantCulture, "he made out the tell ({0}) {1:0.00} s after it began{2}: he expects the blow {3:0.00} s from now and keeps his shield free", Name(g.Kind), now - g.Began, byCount ? ", by the count" : "", expected - now));
            }

            // (And an opening he knows it leaves, if his blow can still land in it.)
            if (!d.Taker.Pending && (d.blow == null || d.blow.Done) && d.Taker.Knows(Name(g.Kind), out float from, out float to) && to - from > 0.05f && g.Began + to > now)
            {
                if (d.Taker.Foresee(now, g.Began + from, g.Began + to, Name(g.Kind) + " as he made it out"))
                {
                    d.Say("READ IT: strikes into " + Name(g.Kind), now, 2f);
                    Told?.Invoke(d, d.Taker.How);
                }
            }
        }

        /// <summary>The other throws a real blow: what a fighter learns from it, and what one who only reacts does.</summary>
        private void SeesBlow(Dancer d, float now, float madeOutIn)
        {
            // (What came before it: the tell, how long before, and how many gestures before that.)
            if (d.seen.Count > 0)
            {
                var last = d.seen[d.seen.Count - 1];
                float delay = now - last.began;
                int count = d.seen.Count - 1;
                float beat = count >= 1 ? last.began - d.seen[d.seen.Count - 2].began : 0f;
                bool clean = delay < 3f;
                for (int i = 1; i < d.seen.Count; i++)
                {
                    clean &= beat <= 0f || Mathf.Abs(d.seen[i].began - d.seen[i - 1].began - beat) < 0.3f;
                }

                d.cycles.Add((last.kind, delay, count, beat, clean));
                Learn(d);
            }

            d.seen.Clear();
            if (d.coverFrom >= 0f && now <= d.coverUntil)
            {
                // (It came as he expected: he covers until it is over.)
                d.expectedCame = true;
                d.coverUntil = now + 1.4f;
            }
            else
            {
                // (Not expected: it is a real blow once it has gone past where a fake would have stopped; he covers from then.)
                d.coverFrom = now + madeOutIn + d.Taker.Reaction;
                d.coverUntil = now + 1.4f;
                d.expectedCame = true;
                d.coverByTell = false;
            }
        }

        private static void Learn(Dancer d)
        {
            int n = d.cycles.Count;
            if (n < LearnsAfter)
            {
                return;
            }

            bool sameTell = true, sameCount = true;
            for (int i = n - LearnsAfter + 1; i < n; i++)
            {
                sameTell &= d.cycles[i].tell == d.cycles[n - LearnsAfter].tell && Mathf.Abs(d.cycles[i].delay - d.cycles[n - LearnsAfter].delay) < 0.35f;
                sameCount &= d.cycles[i].count == d.cycles[n - LearnsAfter].count;
            }

            for (int i = n - LearnsAfter; i < n; i++)
            {
                sameCount &= d.cycles[i].clean && d.cycles[i].count >= 1;
            }

            var newest = d.cycles[n - 1];
            d.knowsTell = sameTell;
            if (sameTell)
            {
                d.knownTell = newest.tell;
                d.knownDelay = newest.delay;
            }

            d.knowsCount = sameTell && sameCount;
            if (d.knowsCount)
            {
                d.knownCount = newest.count;
                d.knownBeat = newest.beat;
            }
        }

        // ---- His blows -----------------------------------------------------------------------------------------------

        private void StepBlow(Dancer d, float now, bool foeOpen)
        {
            var blow = d.blow;
            if (blow != null && !blow.Done)
            {
                if (!d.Unit.Clip.Striking && now - blow.AskedAt >= AimTest.LeastFollowed)
                {
                    blow.Done = true;
                    var foe = d.Foe;
                    if (blow.Landed)
                    {
                        d.Landed++;
                        d.landedAt.Add(now);
                        foe.Taken++;
                        foe.CaughtInFake += blow.CaughtInFake ? 1 : 0;
                    }
                    else if (blow.Met)
                    {
                        d.Stopped++;
                    }
                    else
                    {
                        d.Missed++;
                    }

                    if (blow.Why == "pattern")
                    {
                        foe.ExpectedBlows += blow.Expected ? 1 : 0;
                    }

                    d.Say(blow.Landed ? (blow.CaughtInFake ? "LANDED: caught him in his fake" : "LANDED") : blow.Met ? (blow.Expected ? "stopped: he expected it" : "stopped by his " + (blow.Shield ? "shield" : "sword")) : "met nothing", now, 1.6f);
                    d.restUntil = now + 0.4f;
                    Restart(d, -1f);
                    if (Diagnostics.PlaytestTrace.Enabled)
                    {
                        Diagnostics.PlaytestTrace.Note(d.Unit, "dance", blow.Verdict);
                    }

                    Struck?.Invoke(blow);
                }

                return;
            }

            // (A covering that the blow never came for: his tell with nothing after it.)
            if (d.coverFrom >= 0f && now > d.coverUntil)
            {
                if (!d.expectedCame)
                {
                    d.CoveredForNothing++;
                    d.Say("covered for nothing: no blow came", now);
                    d.seen.Clear();
                }

                d.coverFrom = d.coverUntil = -1f;
            }

            if (!d.Taker.Step(now, foeOpen, out float landIn))
            {
                return;
            }

            if (d.Unit.Clip.Busy || !d.Unit.CanAct || now - d.lastMet < 0.5f * Settles)
            {
                d.Say("could not strike just then", now);
                return;
            }

            Strike(d, now, d.Taker.Read ? "read" : "saw", landIn, d.Taker.How);
        }

        private bool Strike(Dancer d, float now, string why, float landIn, string how)
        {
            if (!d.Unit.AimStrike(d.Foe.Unit, AimSpot.Chest, out _, out _, null, null, null, landIn))
            {
                return false;
            }

            if (d.Now != null && !d.Now.Over)
            {
                Finish(d, now - d.Now.Began);
            }

            d.blow = new DanceBlow { By = d, AskedAt = now, Why = why, How = how };
            d.Blows.Add(d.blow);
            d.ReadBlows += why == "read" ? 1 : 0;
            d.Say(why == "pattern" ? "HIS BLOW" : why == "read" ? "STRIKES: by what he read" : "STRIKES: saw it open", now, 2f);
            // (What the other makes of it: a cut is known for real once it has gone past where a fake would have stopped.)
            var made = d.Unit.Clip.Made;
            float pace = Mathf.Max(0.05f, d.Unit.Clip.Loco.TimeScale);
            float madeOutIn = made != null ? Mathf.Clamp(made.DriveAt * PostureMaker.FeintDepth / pace + 0.08f, 0.1f, 0.6f) : 0.3f;
            SeesBlow(d.Foe, now, madeOutIn);
            return true;
        }

        // ---- His pattern ---------------------------------------------------------------------------------------------

        private void StepPattern(Dancer d, float now)
        {
            if (d.nextAt < 0f)
            {
                Restart(d, now + 0.3f * (d == A ? 1f : 2f));
            }

            if (d.blow != null && !d.blow.Done)
            {
                return;
            }

            if (d.Covering(now) || d.Taker.Pending)
            {
                // (He keeps his shield free, or has a blow coming: his pattern waits where it is.)
                if (d.pausedAt < 0f)
                {
                    d.pausedAt = now;
                }

                return;
            }

            if (d.pausedAt >= 0f)
            {
                // (It goes on from where it was, a moment after; a blow of his that was due is thrown late, or dropped if it is long past.)
                float waited = now - d.pausedAt;
                d.pausedAt = -1f;
                if (d.blowAt >= 0f && waited > 1.5f)
                {
                    Restart(d, now + 0.3f);
                }
                else
                {
                    d.nextAt = Mathf.Max(d.nextAt, now + 0.3f);
                    d.blowAt = d.blowAt >= 0f ? Mathf.Max(d.blowAt, now + 0.2f) : -1f;
                }
            }

            if (d.blowAt >= 0f)
            {
                // (His tell is thrown: his blow comes on its time, as soon as he is free for it.)
                if (now >= d.blowAt && (d.Now == null || d.Now.Over || now - d.Now.Began >= d.Now.Lasts) && Free(d, now))
                {
                    if (!Strike(d, now, "pattern", 0f, ""))
                    {
                        if (now - d.blowAt > 1f)
                        {
                            Restart(d, now + 0.3f);
                        }
                    }
                }
                else if (now - d.blowAt > 1.5f)
                {
                    Restart(d, now + 0.3f);
                }

                return;
            }

            if (now < d.nextAt || !Free(d, now))
            {
                return;
            }

            if (now - d.nextAt > BeatLost && d.place > 0)
            {
                // (The beat is lost: struck, or off his spot. His pattern begins again.)
                Restart(d, now);
            }

            if (d.planned < 0)
            {
                d.planned = d.Pattern.MixedUp ? 1 + dice.Next(3) : Mathf.Max(1, d.Pattern.Count);
            }

            bool tell = d.place >= d.planned;
            var kind = tell ? d.Pattern.Tell : d.Pattern.Fillers[d.place % Mathf.Max(1, d.Pattern.Fillers.Length)];
            if (!Throw(d, kind, tell, now))
            {
                return;
            }

            d.place++;
            if (tell)
            {
                d.tellThrown = true;
                // (Mixed up, one tell in three has no blow after it.)
                bool bluff = d.Pattern.MixedUp && dice.Next(3) == 0;
                if (bluff)
                {
                    Restart(d, now + d.Pattern.Beat);
                }
                else
                {
                    d.blowAt = now + d.Pattern.BlowAfter;
                }
            }
            else
            {
                d.nextAt = now + d.Pattern.Beat;
            }
        }

        /// <summary>He throws one gesture now. False: he could not.</summary>
        public bool Throw(Dancer d, Gesture kind, bool tell, float now)
        {
            var clip = d.Unit.Clip;
            float furthest, lasts;
            switch (kind)
            {
                case Gesture.ShortFake:
                case Gesture.LongFake:
                case Gesture.FakeLunge:
                {
                    float holdWas = PostureMaker.FeintHold;
                    PostureMaker.FeintHold = kind == Gesture.LongFake ? LongHold : kind == Gesture.ShortFake ? 0f : holdWas;
                    PostureMaker.Ask(kind == Gesture.LongFake ? LongDepth : kind == Gesture.ShortFake ? ShortDepth : 0f, kind == Gesture.FakeLunge ? 1f : 0f);
                    bool thrown = d.Unit.AimStrike(d.Foe.Unit, AimSpot.Chest, out _, out _, null, null, "sns_forehand");
                    PostureMaker.FeintHold = holdWas;
                    var made = thrown ? clip.Made : null;
                    if (made == null || !made.Posture)
                    {
                        PostureMaker.Take(out _, out _);
                        return false;
                    }

                    float pace = Mathf.Max(0.05f, clip.Loco.TimeScale);
                    furthest = made.ContactAt / pace;
                    lasts = made.Clip.Duration / pace;
                    break;
                }
                case Gesture.Drop:
                    if (!clip.Drop())
                    {
                        return false;
                    }

                    furthest = PostureMaker.DropDown;
                    lasts = PostureMaker.DropDown + PostureMaker.DropHeld + PostureMaker.DropUp;
                    break;
                default:
                {
                    var pulse = kind == Gesture.BladeDip ? Locomotor.Pulse.BladeDip : kind == Gesture.BladeFlick ? Locomotor.Pulse.BladeFlick : kind == Gesture.BladeCircle ? Locomotor.Pulse.BladeCircle : Locomotor.Pulse.ShieldPress;
                    if (!clip.Bluff(pulse))
                    {
                        return false;
                    }

                    furthest = kind == Gesture.BladeCircle ? 0.25f : 0.12f;
                    lasts = kind == Gesture.BladeDip ? 0.36f : kind == Gesture.BladeFlick ? 0.32f : kind == Gesture.BladeCircle ? 0.55f : 0.48f;
                    break;
                }
            }

            if (d.Now != null && !d.Now.Over)
            {
                Finish(d, now - d.Now.Began);
            }

            d.Now = new DanceGesture { Kind = kind, Tell = tell, Place = d.place, Began = now, Furthest = furthest, Lasts = lasts };
            d.Gestures.Add(d.Now);
            d.openRun = 0;
            d.Say((tell ? "HIS TELL: " : "") + Name(kind), now, Mathf.Max(0.8f, lasts));
            Sees(d.Foe, d.Now, now);
            return true;
        }

        // ---- Counted -------------------------------------------------------------------------------------------------

        public int Gestures(Dancer d) => d.Gestures.Count;

        /// <summary>One fighter's dance so far, counted.</summary>
        public string Tally(Dancer d)
        {
            int tells = 0;
            foreach (var g in d.Gestures)
            {
                tells += g.Tell ? 1 : 0;
            }

            int blows = d.Landed + d.Stopped + d.Missed;
            return $"{d.Gestures.Count} gestures ({tells} tells), {blows} blows: {d.Landed} landed, {d.Stopped} stopped, {d.Missed} met nothing ({d.ReadBlows} of them thrown by what he read); "
                + $"took {d.Taken} ({d.CaughtInFake} while in a fake of his own); expected {d.ExpectedBlows} of the other's pattern blows; covered for nothing {d.CoveredForNothing} times; the fight goes {d.Going(lastNow):+0;-0;0} for him";
        }
    }
}
