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

        /// <summary>A dodge: he ducks and leans aside, and comes back up.</summary>
        Dodge,
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

        /// <summary>The gestures thrown before the tell, in turn, round and round. All the sword arm's or the body's: the shield keeps its cover.</summary>
        public Gesture[] Fillers = { Gesture.BladeDip, Gesture.ShortFake };

        /// <summary>The parts his blows go for, in turn: of those not covered as he strikes, the next; all covered, the next whatever.</summary>
        public AimSpot[] Spots = { AimSpot.Head, AimSpot.ThighR, AimSpot.ShoulderR, AimSpot.ShinL };

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

                var parts = new List<string>();
                foreach (var spot in Spots)
                {
                    parts.Add(AimSpots.Name(spot));
                }

                return (MixedUp ? "one to three of " : Count + " of ") + string.Join(" / ", kinds) + ", then " + Dance.Name(Tell) + ", then his blow, at the " + string.Join(", ", parts) + " in turn (whichever is not covered)"
                    + string.Format(System.Globalization.CultureInfo.InvariantCulture, "; a gesture every {0:0.0} s", Beat) + (MixedUp ? "; mixed up" : "; always the same");
            }
        }

        /// <summary>The pattern that goes with one of the made-up fighters (<see cref="FighterLevers"/>): tries, not styles decided on.</summary>
        public static GesturePattern Named(string name)
        {
            switch ((name ?? "").Trim().ToLowerInvariant())
            {
                case "prowler":
                    // (Quick small feelers, then he sinks before he springs: low, at the legs and feet.)
                    return new GesturePattern { Name = "Prowler", Fillers = new[] { Gesture.BladeFlick, Gesture.Dodge, Gesture.BladeDip, Gesture.ShortFake }, Count = 3, Tell = Gesture.Drop, Beat = 1.2f, BlowAfter = 0.45f,
                        Spots = new[] { AimSpot.ShinL, AimSpot.ThighR, AimSpot.FootR, AimSpot.ThighL, AimSpot.ShinR, AimSpot.Head } };
                case "strutter":
                    // (Showy: a blade circle, a dip and a short fake, and a long fake slash before the real one: high, at the head and shoulders.)
                    return new GesturePattern { Name = "Strutter", Fillers = new[] { Gesture.BladeCircle, Gesture.BladeDip, Gesture.ShortFake }, Count = 3, Tell = Gesture.LongFake, Beat = 1.5f, BlowAfter = 1.2f,
                        Spots = new[] { AimSpot.Head, AimSpot.ShoulderR, AimSpot.Neck, AimSpot.UpperArmR, AimSpot.ThighR } };
                case "bull":
                    // (Few frills: two flicks of the point, and it dips before he cuts: at the body and the thighs.)
                    return new GesturePattern { Name = "Bull", Fillers = new[] { Gesture.BladeFlick }, Count = 2, Tell = Gesture.BladeDip, Beat = 1.9f, BlowAfter = 0.3f,
                        Spots = new[] { AimSpot.ThighR, AimSpot.Belly, AimSpot.ShoulderR, AimSpot.ThighL, AimSpot.Hips } };
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

        /// <summary>As it met, the other was winding up a blow of his own that had not landed yet.</summary>
        public bool CaughtWinding;

        public bool Landed => Met && !Shield && !Parried;

        /// <summary>Its class (light, heavy; plain with the classes off), and how long after it was asked for its blade was to land (seconds).</summary>
        public AttackClass Kind;
        public float LandsIn;

        /// <summary>What it did as it landed: the speed of the blade, and the harm.</summary>
        public float Speed, Harm;

        /// <summary>The part it was thrown at.</summary>
        public AimSpot Spot = AimSpot.Chest;

        /// <summary>It landed on the part it was thrown at.</summary>
        public bool OnSpot => Landed && AimSpots.Counts(Spot, Part);

        public string Verdict => (Kind == AttackClass.Plain ? "" : "a " + AttackClasses.Name(Kind) + " blow ") + "at the " + AimSpots.Name(Spot) + ", " + (Why == "stop" ? "thrown into the other's wind-up" : Why == "pattern" ? "his pattern's blow" : Why == "read" ? "thrown by what he has read" : Why == "answer" ? "his answer to a blow he stopped" : "thrown at a chest he saw open")
            + ": " + (!Met ? "met nothing" : Shield ? "met the shield" : Parried ? "was parried" : "LANDED on the " + Part)
            + (Met && CaughtInFake ? " (the other was in a fake of his own)" : "") + (Met && CaughtWinding ? " (the other was still winding up his own)" : "") + (Met && Expected ? " (the other expected it and was covering)" : "")
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
        internal int place, planned = -1, spotTurn;
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
        internal float coverFrom = -1f, coverUntil = -1f, expectedAt, pausedAt = -1f, answerAt = -1f, stopAt = -1f, dodgeAt = -1f;

        /// <summary>Blows he threw into the other's wind-up; dodges he threw at a blow he saw coming.</summary>
        public int StopHits, DodgedBlows;

        /// <summary>His blows by class (light, heavy): thrown, landed, and what the landed ones did in all (blade speed, harm).</summary>
        public readonly int[] ClassThrown = new int[3], ClassLanded = new int[3];
        public readonly float[] ClassLandsIn = new float[3], ClassSpeed = new float[3], ClassHarm = new float[3];
        internal bool expectedCame, coverByTell;
        internal int openRun;
        internal float openRunAt, runFrom;
        internal bool runOpen;

        /// <summary>He expects a blow and keeps his shield free: no gesture, no blow of his pattern.</summary>
        public bool Covering(float now) => coverFrom >= 0f && now >= coverFrom && now <= coverUntil;

        /// <summary>He is covering because he made out the other's tell (not only because he saw the blow start).</summary>
        public bool Expecting(float now) => Covering(now) && coverByTell;

        /// <summary>Blows of his that landed on the other's body, that met its shield or sword, that met nothing; blows of the other's that landed on him.</summary>
        public int Landed, Stopped, Missed, Taken;

        /// <summary>Of his blows that landed: how many on the part they were thrown at; and where they landed, counted by part.</summary>
        public int OnSpot;
        public readonly Dictionary<PartId, int> LandedOn = new Dictionary<PartId, int>();

        /// <summary>Of the blows that landed on him: how many while he was in a fake of his own; how many of the other's blows he had expected.</summary>
        public int CaughtInFake, ExpectedBlows, CoveredForNothing, ReadBlows;

        /// <summary>Blows he threw in answer to one he stopped; of the blows that landed on him, how many were such answers.</summary>
        public int Answered, TakenAnswers;

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

        // ---- His feet (the dance's third part) ----
        internal float spellUntil, outUntil, gaveGround, tempoFor;
        internal float radialWas, stoodAt = -10f;
        internal bool wasGoing;
        internal int circling, lastSide = 1;
        internal bool wantsIn, wantedIn;
        internal float leaptForCover = -1f, leapUntil;

        /// <summary>Leaps in for a blow and leaps out from one he expected, so far.</summary>
        public int LeaptIn, LeaptOut;

        /// <summary>What he is about, by the gap and by how the fight is going: coming in, probing, pressing, shelled up.</summary>
        public string Tempo = "";

        /// <summary>How far he has walked in the dance (metres), and how long he was in each tempo (seconds).</summary>
        public float Walked;
        public readonly Dictionary<string, float> TempoTime = new Dictionary<string, float>();
        internal Vector3 lastAt;
        internal bool lastAtKnown;

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

        /// <summary>What he learns of the other's pattern he learns from this many of its last blows.</summary>
        public const int LearnsFrom = 4;

        /// <summary>
        /// The dance is paced by the gap (Combat Feel "The Dance Is Paced By The Gap"; the dance's third part). Each
        /// fighter keeps the distance he likes, between just outside reach (<see cref="Edge"/>) and inside it
        /// (<see cref="Reach"/>) by how aggressive he is; goes round the other in spells and stands between them, by
        /// how restless he is; comes in for his blow and goes out after it; and one who is not aggressive steps out
        /// of reach when he sees the other's tell. How the fight is going shifts it: ahead he presses nearer, behind
        /// he keeps further off. Off: they stand on their spots, as in the second part.
        /// </summary>
        public static bool Moves = true;

        /// <summary>
        /// Calmer feet in the dance (the plan "Convincing walking in the dance", step 4; Combat Feel "The Dance Asks
        /// For Steps"). A dancer does not set off toward the other or away from him for less than a step's worth of
        /// ground (<see cref="StepsWorth"/>); once he has set off he goes all the way to the distance he likes, not
        /// only until he is nearly there; and once he has come to a stand he stands for at least
        /// <see cref="StandsAtLeast"/> seconds. What cannot wait is not held: the other too near, a blow of his own to
        /// come in for, a blow to step out of. Off: as before, he sets off for 12 cm and stops when 12 cm off.
        /// </summary>
        public static bool CalmFeet = true;

        /// <summary>Calm feet: the least he sets off in or out for (metres at a Medium's size), and the least he stands once stopped (seconds).</summary>
        public static float StepsWorth = 0.3f, StandsAtLeast = 0.6f;

        /// <summary>For the reports: how often a dancer set off from a stand; and how often one who was going stopped because he could not act (a gesture, a blow, a hit taken) or because he had nowhere he wanted to go.</summary>
        public int SetOffs, StopsBusy, StopsThere;

        /// <summary>For the reports: how long, both dancers together, something pressed (the other too near, a blow to come in for or to step out of): calm feet hold nothing then.</summary>
        public float UrgentFor;

        /// <summary>Root to root, at a Medium's size (metres): where a blow reaches; just outside it; too near; and how far in a blow on the move may be started from.</summary>
        public const float Reach = 1.5f, Edge = 2.2f, TooNear = 1.1f, StrikesFrom = 1.95f;

        /// <summary>Where the dance is danced, and how far from there a fighter may back before he must go round instead (metres).</summary>
        public Vector3 Centre;
        public float Room = 3.5f;

        /// <summary>How far apart the two are (metres, root to root, flat).</summary>
        public float Gap
        {
            get
            {
                if (!Ready)
                {
                    return 0f;
                }

                Vector3 to = B.Unit.Root.position - A.Unit.Root.position;
                to.y = 0f;
                return to.magnitude;
            }
        }

        /// <summary>The gap so far: its sum over the steps, the steps, its least and most.</summary>
        public float GapSum, GapLeast = float.MaxValue, GapMost;
        public int GapSteps;

        /// <summary>
        /// A blow stopped is answered (Combat Feel "A Stopped Blow Is Answered"): the one whose shield or sword stopped a
        /// blow strikes back at once, while the other is still in it. It is what a blow wasted on a fake costs. An answer
        /// is not itself answered.
        /// </summary>
        public static bool Answers = false;

        /// <summary>How far into the wind-up a short fake and a long one go (of a feint's own depth), and how long the long one is held (the fakes test's own).</summary>
        public static float ShortDepth => FakeTest.ShortDepth;
        public static float LongDepth => FakeTest.LongDepth;
        public static float LongHold => FakeTest.LongHold;

        public event System.Action<Dancer, DanceGesture> Shown;
        public event System.Action<DanceBlow> Struck;
        public event System.Action<Dancer, string> Told;

        private readonly System.Random dice = new System.Random(11);
        private float lastNow;
        private bool wasOn, feetMoved;

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
                case Gesture.Dodge: return "a dodge";
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
            wasOn = feetMoved = false;
            GapSum = GapMost = 0f;
            GapLeast = float.MaxValue;
            GapSteps = 0;
            foreach (var d in new[] { A, B })
            {
                d.Now = null;
                d.Gestures.Clear();
                d.Blows.Clear();
                d.blow = null;
                d.Note = "";
                d.Landed = d.Stopped = d.Missed = d.Taken = d.CaughtInFake = d.ExpectedBlows = d.CoveredForNothing = d.ReadBlows = d.Answered = d.TakenAnswers = 0;
                d.answerAt = d.pausedAt = -1f;
                d.OnSpot = d.spotTurn = d.LeaptIn = d.LeaptOut = d.StopHits = d.DodgedBlows = 0;
                d.stopAt = d.dodgeAt = -1f;
                System.Array.Clear(d.ClassThrown, 0, 3);
                System.Array.Clear(d.ClassLanded, 0, 3);
                System.Array.Clear(d.ClassLandsIn, 0, 3);
                System.Array.Clear(d.ClassSpeed, 0, 3);
                System.Array.Clear(d.ClassHarm, 0, 3);
                d.leaptForCover = -1f;
                d.leapUntil = 0f;
                d.wantedIn = false;
                d.LandedOn.Clear();
                d.spellUntil = d.outUntil = d.gaveGround = d.Walked = 0f;
                d.radialWas = 0f;
                d.stoodAt = -10f;
                d.wasGoing = false;
                d.circling = 0;
                d.wantsIn = d.lastAtKnown = false;
                d.TempoTime.Clear();
                d.Tempo = "";
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
            return !clip.Busy && d.Unit.CanAct && !clip.IsUncovering && !clip.ShieldArmHeld && (Moves || OnItsSpot(d.Unit)) && now - d.lastMet >= Settles && now >= d.restUntil;
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
            blow.Speed = hit.Speed;
            blow.Harm = hit.Shield || hit.Parried ? 0f : hit.Result.Damage * AttackClasses.PowerOf(blow.Kind);
            blow.CaughtWinding = foe.blow != null && !foe.blow.Done && !foe.blow.Met && now - foe.blow.AskedAt < foe.blow.LandsIn;
            blow.Expected = foe.Expecting(now);
            if (Answers && (hit.Shield || hit.Parried) && blow.Why != "answer" && !blow.CaughtInFake)
            {
                // (Stopped: the other answers it, after his reaction time.)
                foe.answerAt = now + foe.Taker.Reaction;
            }
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
                if (wasOn)
                {
                    StopFeet(A);
                    StopFeet(B);
                    wasOn = false;
                }

                return;
            }

            if (!wasOn)
            {
                wasOn = true;
                Centre = 0.5f * (A.Unit.Root.position + B.Unit.Root.position);
            }

            float gap = Gap;
            GapSum += gap;
            GapSteps++;
            GapLeast = Mathf.Min(GapLeast, gap);
            GapMost = Mathf.Max(GapMost, gap);
            if (Moves)
            {
                StepFeet(A, now, dt, gap);
                StepFeet(B, now, dt, gap);
                // (A shield bashes what comes too near: the two are the lab's dummies, so the dance asks it for them.)
                A.Unit.Clip.BashIfNear(B.Unit.Clip);
                B.Unit.Clip.BashIfNear(A.Unit.Clip);
            }
            else if (feetMoved)
            {
                StopFeet(A);
                StopFeet(B);
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

            // (His chest's longest opening in the gesture is the one that counts: a flicker of it is not one to strike into.)
            if (d.openRun == 3)
            {
                d.runFrom = d.openRunAt;
                d.runOpen = true;
            }

            if (d.runOpen)
            {
                float until = d.openRun > 0 ? t : d.openRunAt;
                if (!g.Opened || until - d.runFrom > g.ShutAt - g.OpenedAt)
                {
                    g.Opened = true;
                    g.OpenedAt = d.runFrom;
                    g.ShutAt = until;
                    g.Shut = d.openRun <= -3;
                }

                if (d.openRun <= -3)
                {
                    d.runOpen = false;
                }
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

            g.Shut = g.Opened;
            g.Over = true;
            // (What it left open is what the other saw of it: the next of its kind he knows. One he struck into shuts
            // differently, so it does not change what he knows; but if it is all he has seen of its kind, he goes by it.)
            bool knew = d.Foe.Taker.Knows(Name(g.Kind), out _, out _);
            if (!g.StruckIn || !knew)
            {
                d.Foe.Taker.Met(Name(g.Kind), g.Opened ? g.OpenedAt : 0f, g.Opened ? g.ShutAt : 0f);
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
            else if (!d.Taker.Pending && (d.blow == null || d.blow.Done))
            {
                // (A fake slash he cannot tell for short or long yet: he takes it for a short one until it goes on.)
                Meets(d, Gesture.ShortFake, now);
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
            if (!d.Taker.Pending && (d.blow == null || d.blow.Done))
            {
                if (d.Taker.Knows(Name(g.Kind), out float from, out float to) && to - from > 0.05f && g.Began + to > now)
                {
                    if (d.Taker.Foresee(now, g.Began + from, g.Began + to, Name(g.Kind) + " as he made it out"))
                    {
                        d.Say("READ IT: strikes into " + Name(g.Kind), now, 2f);
                        Told?.Invoke(d, d.Taker.How);
                    }
                }
                else
                {
                    Meets(d, g.Kind, now);
                }
            }
        }

        /// <summary>
        /// A gesture he has no blow planned for. One he has not met he cannot read: he goes for what he sees open, as
        /// one who does not read would, and learns what it leaves open. One he has met, and knows leaves nothing worth
        /// a blow, he lets go.
        /// </summary>
        private static void Meets(Dancer d, Gesture kind, float now)
        {
            if (d.Taker.Knows(Name(kind), out _, out _))
            {
                d.Taker.Done();
            }
            else
            {
                d.Taker.Watch(now, "he had not met " + Name(kind) + " before, so he went for what he saw open");
            }
        }

        /// <summary>The other throws a real blow: what a fighter learns from it, and what one who only reacts does.</summary>
        private void SeesBlow(Dancer d, float now, float madeOutIn, bool learns, DanceBlow coming)
        {
            // (What he may do about it once he has made it out, besides covering: strike into its wind-up, if it is a
            // slow one and he is near enough and bold enough; or duck under it, if it comes high and he is one who dodges.)
            // (One who was waiting for it, having seen the tell, knows it for the blow as it begins.)
            float sees = now + (d.Expecting(now) ? 0f : madeOutIn) + d.Taker.Reaction;
            d.stopAt = d.dodgeAt = -1f;
            if (coming != null && (d.blow == null || d.blow.Done))
            {
                float left = now + coming.LandsIn - sees;
                bool high = coming.Spot == AimSpot.Head || coming.Spot == AimSpot.Neck || coming.Spot == AimSpot.ShoulderL || coming.Spot == AimSpot.ShoulderR;
                if (AttackClasses.On && left > LightLandsIn + 0.05f && Aggressive(d, now) >= 0.5f)
                {
                    d.stopAt = sees;
                }
                else if (high && left > PostureMaker.DodgeDown * 0.7f && dice.NextDouble() < Dodges(d))
                {
                    d.dodgeAt = sees;
                }
            }

            // (What came before it: the tell, how long before, and how many gestures before that. Not from an answer to
            // a blow of his own that was stopped: he knows what that one came from.)
            if (learns && d.seen.Count > 0)
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

        /// <summary>
        /// What he makes of the other's last blows. Not every blow of the other's is his pattern's (some he throws at
        /// what he sees open, some in answer to a blow stopped), so the tell is the gesture that came the same time
        /// before at least two of his last <see cref="LearnsFrom"/> blows; and the count is known when the last two
        /// of those had the same number of gestures before them, evenly spaced.
        /// </summary>
        private static void Learn(Dancer d)
        {
            int n = d.cycles.Count, from = Mathf.Max(0, n - LearnsFrom);
            int best = 0;
            Gesture tell = default;
            float delay = 0f;
            for (int i = n - 1; i >= from; i--)
            {
                if (d.cycles[i].delay > 2.5f)
                {
                    continue;
                }

                int same = 0;
                float sum = 0f;
                for (int k = from; k < n; k++)
                {
                    if (d.cycles[k].tell == d.cycles[i].tell && Mathf.Abs(d.cycles[k].delay - d.cycles[i].delay) < 0.25f)
                    {
                        same++;
                        sum += d.cycles[k].delay;
                    }
                }

                if (same > best)
                {
                    best = same;
                    tell = d.cycles[i].tell;
                    delay = sum / same;
                }
            }

            d.knowsTell = best >= LearnsAfter;
            d.knowsCount = false;
            if (!d.knowsTell)
            {
                return;
            }

            d.knownTell = tell;
            d.knownDelay = delay;
            int found = 0, count = -1;
            float beat = 0f;
            bool even = true;
            for (int i = n - 1; i >= from && found < 2; i--)
            {
                var c = d.cycles[i];
                if (c.tell != tell || Mathf.Abs(c.delay - delay) >= 0.25f)
                {
                    continue;
                }

                even &= c.clean && c.count >= 1 && (count < 0 || c.count == count);
                if (count < 0)
                {
                    count = c.count;
                    beat = c.beat;
                }

                found++;
            }

            d.knowsCount = found >= 2 && even;
            if (d.knowsCount)
            {
                d.knownCount = count;
                d.knownBeat = beat;
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
                    int k = (int)blow.Kind;
                    d.ClassThrown[k]++;
                    d.ClassLandsIn[k] += blow.LandsIn;
                    if (blow.Landed)
                    {
                        d.ClassLanded[k]++;
                        d.ClassSpeed[k] += blow.Speed;
                        d.ClassHarm[k] += blow.Harm;
                    }
                    if (blow.Landed)
                    {
                        d.Landed++;
                        d.OnSpot += blow.OnSpot ? 1 : 0;
                        d.LandedOn.TryGetValue(blow.Part, out int times);
                        d.LandedOn[blow.Part] = times + 1;
                        d.landedAt.Add(now);
                        foe.Taken++;
                        foe.CaughtInFake += blow.CaughtInFake ? 1 : 0;
                        foe.TakenAnswers += blow.Why == "answer" ? 1 : 0;
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
                    d.wantsIn = false;
                    // (Out again after it, sooner back the more aggressive he is.)
                    d.outUntil = now + Mathf.Lerp(1.8f, 0.3f, Aggressive(d, now));
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

            if (d.stopAt >= 0f && now >= d.stopAt)
            {
                // (Into his wind-up: a light blow, at once, if he is near enough for it to reach.)
                d.stopAt = -1f;
                if (!d.Unit.Clip.Busy && d.Unit.CanAct && (!Moves || Gap <= StrikesFrom * Size(d)) && Strike(d, now, "stop", 0f, ""))
                {
                    d.Taker.Done();
                    return;
                }
            }

            if (d.dodgeAt >= 0f && now >= d.dodgeAt)
            {
                d.dodgeAt = -1f;
                if (d.Unit.Clip.Dodge(dice.Next(2) == 0 ? -1 : 1))
                {
                    d.DodgedBlows++;
                    d.Say("DODGES", now, 1f);
                }
            }

            if (d.answerAt >= 0f && now >= d.answerAt)
            {
                // (His answer to a blow he stopped: as quick as a blow can be. Dropped if he is not free for it in time.)
                if (!d.Unit.Clip.Busy && d.Unit.CanAct && Strike(d, now, "answer", OpeningTaker.Quickest, ""))
                {
                    d.answerAt = -1f;
                    d.Taker.Done();
                    return;
                }

                if (now - d.answerAt > 0.35f)
                {
                    d.answerAt = -1f;
                }
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

            if (Moves && Gap > StrikesFrom * Size(d))
            {
                d.Say("saw his chance, out of reach", now);
                return;
            }

            Strike(d, now, d.Taker.Read ? "read" : "saw", landIn, d.Taker.How);
        }

        /// <summary>The part his next blow goes for: the next in his turn that the other's shield does not cover as he sees it now; all covered, the next whatever.</summary>
        private AimSpot NextSpot(Dancer d)
        {
            var spots = d.Pattern.Spots;
            if (spots == null || spots.Length == 0)
            {
                return AimSpot.Chest;
            }

            Vector3 from = d.Unit.Clip.PartAt(PartId.UpperArmR);
            for (int i = 0; i < spots.Length; i++)
            {
                var spot = spots[(d.spotTurn + i) % spots.Length];
                if (Openings.LookAt(d.Foe.Unit.Clip, from, spot).Open)
                {
                    d.spotTurn = (d.spotTurn + i + 1) % spots.Length;
                    return spot;
                }
            }

            var next = spots[d.spotTurn % spots.Length];
            d.spotTurn = (d.spotTurn + 1) % spots.Length;
            return next;
        }

        private bool Strike(Dancer d, float now, string why, float landIn, string how)
        {
            // (A blow of his pattern, or an answer, goes for a part of his own choosing; one thrown into an opening he
            // saw or read goes for the chest that opening is.)
            var spot = why == "pattern" || why == "answer" ? NextSpot(d) : why == "stop" ? AimSpot.UpperArmR : AimSpot.Chest;
            // (His pattern's blow is heavy or light by how he fights; every blow thrown to take a chance, or in answer,
            // is a light one: quick enough to get there.)
            var kind = !AttackClasses.On ? AttackClass.Plain : why == "pattern" ? (dice.NextDouble() < Heavy(d) ? AttackClass.Heavy : AttackClass.Light) : AttackClass.Light;
            if (!d.Unit.AimStrike(d.Foe.Unit, spot, out _, out _, null, null, null, landIn, null, kind))
            {
                return false;
            }

            if (d.Now != null && !d.Now.Over)
            {
                Finish(d, now - d.Now.Began);
            }

            d.blow = new DanceBlow { By = d, AskedAt = now, Why = why, How = how, Spot = spot, Kind = kind };
            d.Blows.Add(d.blow);
            d.ReadBlows += why == "read" ? 1 : 0;
            d.Answered += why == "answer" ? 1 : 0;
            d.StopHits += why == "stop" ? 1 : 0;
            d.Say(why == "pattern" ? (kind == AttackClass.Heavy ? "HIS BLOW: a heavy one" : kind == AttackClass.Light ? "HIS BLOW: a light one" : "HIS BLOW") : why == "stop" ? "STRIKES INTO his wind-up" : why == "read" ? "STRIKES: by what he read" : why == "answer" ? "ANSWERS the blow he stopped" : "STRIKES: saw it open", now, 2f);
            // (What the other makes of it: a cut is known for real once it has gone past where a fake would have stopped.)
            var made = d.Unit.Clip.Made;
            float pace = Mathf.Max(0.05f, d.Unit.Clip.Loco.TimeScale);
            float madeOutIn = made != null ? Mathf.Clamp(made.DriveAt * PostureMaker.FeintDepth / pace + 0.08f, 0.1f, 0.6f) : 0.3f;
            d.blow.LandsIn = made != null ? made.ContactAt / pace : 0.6f;
            SeesBlow(d.Foe, now, madeOutIn, why != "answer" && why != "stop", d.blow);
            return true;
        }

        // ---- His feet -----------------------------------------------------------------------------------------------

        private static float Size(Dancer d) => Mathf.Sqrt(Mathf.Max(0.1f, d.Unit.Body.SizeFactor));

        /// <summary>How aggressive he is now (0 to 1): his own, and more when the fight goes his way, less when it goes against him.</summary>
        public float Aggressive(Dancer d, float now)
        {
            var own = d.Unit.Clip != null ? d.Unit.Clip.Loco.Own : null;
            float mine = FighterLevers.On && own != null ? own.Aggressive : 0.5f;
            int going = d.Going(now);
            return Mathf.Clamp01(mine + (going >= 2 ? 0.25f : going <= -2 ? -0.3f : 0f));
        }

        /// <summary>A light blow lands about this long after it is asked for (seconds): what a blow into a wind-up needs.</summary>
        public const float LightLandsIn = 0.42f;

        private static float Heavy(Dancer d)
        {
            var own = d.Unit.Clip != null ? d.Unit.Clip.Loco.Own : null;
            return FighterLevers.On && own != null ? own.Heavy : 0.5f;
        }

        private static float Dodges(Dancer d)
        {
            var own = d.Unit.Clip != null ? d.Unit.Clip.Loco.Own : null;
            return FighterLevers.On && own != null ? own.Dodges : 0.2f;
        }

        private static float Leaps(Dancer d)
        {
            var own = d.Unit.Clip != null ? d.Unit.Clip.Loco.Own : null;
            return FighterLevers.On && own != null ? own.Leaps : 0.3f;
        }

        /// <summary>He leaps that way so far (metres), as the duel lab's leaps are made. False: he could not.</summary>
        private static bool Leap(Dancer d, Vector3 way, float far, float now)
        {
            var unit = d.Unit;
            const float flight = 0.25f;
            unit.DummyGo = null;
            bool leapt = LeapMaker.On ? unit.Clip.LeapMade(way, far / Mathf.Max(0.1f, unit.Body.SizeFactor), flight) : unit.Clip.LeapNow(way, far / Mathf.Max(0.1f, unit.Body.SizeFactor), flight);
            if (!leapt)
            {
                return false;
            }

            unit.DummySpot = unit.Root.position + way * far;
            unit.Clip.WalksOn = false;
            d.leapUntil = now + flight + 0.35f;
            return true;
        }

        private static float Restless(Dancer d)
        {
            var own = d.Unit.Clip != null ? d.Unit.Clip.Loco.Own : null;
            return FighterLevers.On && own != null ? own.Restless : 0.5f;
        }

        private void StopFeet(Dancer d)
        {
            feetMoved = false;
            if (d.Unit == null)
            {
                return;
            }

            d.Unit.DummyGo = null;
            d.Unit.DummySpot = d.Unit.Root.position;
            d.Tempo = "";
            d.wantsIn = false;
            if (d.Unit.Clip != null)
            {
                d.Unit.Clip.WalksOn = false;
            }
        }

        /// <summary>One step of his feet: where he wants to be against the other, and the velocity he is steered by for it.</summary>
        private void StepFeet(Dancer d, float now, float dt, float gap)
        {
            feetMoved = true;
            var unit = d.Unit;
            var clip = unit.Clip;
            Vector3 at = unit.Root.position;
            if (d.lastAtKnown)
            {
                Vector3 went = at - d.lastAt;
                went.y = 0f;
                d.Walked += went.magnitude;
            }

            d.lastAt = at;
            d.lastAtKnown = true;
            Vector3 to = d.Foe.Unit.Root.position - at;
            to.y = 0f;
            if (gap < 1e-3f)
            {
                return;
            }

            Vector3 toward = to / gap, left = Vector3.Cross(toward, Vector3.up);
            float size = Size(d), aggressive = Aggressive(d, now), restless = Restless(d);
            int going = d.Going(now);
            d.Tempo = gap > (Edge + 0.8f) * size ? "coming in" : going >= 2 ? "pressing" : going <= -2 ? "shelled up" : "probing";
            d.TempoTime.TryGetValue(d.Tempo, out float so);
            d.TempoTime[d.Tempo] = so + dt;

            // (A blow he has read is coming up: he comes in for it ahead of time.)
            bool comesIn = d.wantsIn || d.Taker.Pending;
            // (He sees the other's tell: one who is not aggressive steps out of reach of the blow instead of only covering;
            // and one who leaps readily leaps out, if the blow could reach him where he is.)
            if (d.Expecting(now) && aggressive < 0.6f)
            {
                d.outUntil = Mathf.Max(d.outUntil, d.coverUntil - 0.6f);
                if (d.coverFrom != d.leaptForCover)
                {
                    d.leaptForCover = d.coverFrom;
                    if (gap < Edge * size && dice.NextDouble() < Leaps(d) && Leap(d, -toward, Mathf.Clamp(Edge * size + 0.2f - gap, 0.4f, 0.8f), now))
                    {
                        d.LeaptOut++;
                        d.Say("LEAPS OUT of his blow", now, 1.2f);
                    }
                }
            }

            // (His blow is due and he is still well out of reach: one who leaps readily leaps the last of the way in.)
            if (d.wantsIn && !d.wantedIn && gap > (StrikesFrom + 0.2f) * size && dice.NextDouble() < Leaps(d)
                && Leap(d, toward, Mathf.Clamp(gap - Reach * size, 0.4f, 0.8f), now))
            {
                d.LeaptIn++;
                d.Say("LEAPS IN for his blow", now, 1.2f);
            }

            d.wantedIn = d.wantsIn;
            if (now < d.leapUntil)
            {
                // (In the air: his feet are the leap's.)
                return;
            }

            float likes = Mathf.Lerp(Edge + 0.2f, Reach + 0.1f, aggressive);
            if (now < d.outUntil && !comesIn)
            {
                likes = Mathf.Max(likes, Edge + 0.3f);
            }

            if (comesIn)
            {
                likes = Reach;
            }

            likes *= size;
            float off = gap - likes, radial = 0f;
            if (off > 0.12f)
            {
                radial = Mathf.Min(off * 2.2f + 0.2f, comesIn ? 1.6f : 1.1f);
            }
            else if (off < -0.3f || (off < -0.12f && now < d.outUntil))
            {
                radial = -Mathf.Min(-off * 2.2f + 0.2f, 0.8f);
            }

            // (Calm feet: not in or out for less than a step's worth; once going, all the way there.)
            bool urgent = gap < TooNear * size || comesIn || now < d.outUntil || d.Expecting(now);
            UrgentFor += urgent ? dt : 0f;
            if (CalmFeet)
            {
                // (Once going, all the way there, whatever he is about; not setting off for less than a step's worth
                // only when nothing presses.)
                float worth = StepsWorth * size;
                if (d.radialWas > 0f && off > 0.03f)
                {
                    radial = Mathf.Max(radial, Mathf.Min(off * 2.2f + 0.2f, comesIn ? 1.6f : 1.1f));
                }
                else if (d.radialWas < 0f && off < -0.03f)
                {
                    radial = Mathf.Min(radial, -Mathf.Min(-off * 2.2f + 0.2f, 0.8f));
                }
                else if (!urgent && d.radialWas == 0f && Mathf.Abs(off) < worth)
                {
                    radial = 0f;
                }
            }

            if (gap < TooNear * size)
            {
                radial = -0.9f;
            }

            // (Ground given is not given for ever: so much of it, and he stands, or goes round.)
            d.gaveGround = Mathf.Max(0f, d.gaveGround - 0.25f * dt);
            bool cornered = false;
            if (radial < 0f)
            {
                Vector3 from = at - Centre;
                from.y = 0f;
                cornered = d.gaveGround > 1.5f || (from.magnitude > Room && Vector3.Dot(from, -toward) > 0f);
                if (cornered && gap >= TooNear * size)
                {
                    radial = 0f;
                }
                else
                {
                    d.gaveGround += -radial * dt;
                }
            }

            // (Round the other in spells, and standing between them.)
            if (now >= d.spellUntil)
            {
                double roll = dice.NextDouble();
                if (d.circling != 0)
                {
                    d.circling = 0;
                    d.spellUntil = now + Mathf.Lerp(1.8f, 0.3f, restless) * (0.6f + 0.8f * (float)roll);
                }
                else
                {
                    d.lastSide = dice.NextDouble() < 0.65 ? d.lastSide : -d.lastSide;
                    d.circling = d.lastSide;
                    d.spellUntil = now + Mathf.Lerp(0.9f, 2.8f, restless) * (0.6f + 0.8f * (float)roll);
                }
            }

            float round = d.circling * 0.7f;
            if (cornered)
            {
                // (Backed as far as he may: he goes round, toward the middle.)
                Vector3 inward = Centre - at;
                inward.y = 0f;
                round = (Vector3.Dot(left, inward) >= 0f ? 1f : -1f) * 0.8f;
            }

            if (comesIn || clip.Striking || gap > (Edge + 0.8f) * size)
            {
                round = 0f;
            }

            Vector3 go = (toward * radial + left * round) * size;
            bool goes = !clip.Fallen && unit.CanAct && go.magnitude >= 0.2f;
            if (CalmFeet && goes && !d.wasGoing && !urgent && now - d.stoodAt < StandsAtLeast)
            {
                // (Calm feet: come to a stand, he stands a moment before he sets off again.)
                goes = false;
                radial = 0f;
            }

            d.radialWas = goes ? Mathf.Sign(radial) * (Mathf.Abs(radial) > 1e-3f ? 1f : 0f) : 0f;
            if (!goes)
            {
                if (d.wasGoing)
                {
                    d.stoodAt = now;
                    if (clip.Fallen || !unit.CanAct)
                    {
                        StopsBusy++;
                    }
                    else
                    {
                        StopsThere++;
                    }
                }

                unit.DummyGo = null;
                unit.DummySpot = at;
                clip.WalksOn = false;
            }
            else
            {
                SetOffs += d.wasGoing ? 0 : 1;
                unit.DummyGo = go;
                clip.WalksOn = true;
            }

            d.wasGoing = goes;
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
                // (Paced by the gap, he comes in for it first, and throws it once he is near enough for it to reach.)
                bool near = !Moves || Gap <= StrikesFrom * Size(d);
                d.wantsIn = Moves && now >= d.blowAt - 0.4f;
                if (now >= d.blowAt && near && (d.Now == null || d.Now.Over || now - d.Now.Began >= d.Now.Lasts) && Free(d, now))
                {
                    if (!Strike(d, now, "pattern", 0f, ""))
                    {
                        if (now - d.blowAt > 1f)
                        {
                            d.wantsIn = false;
                            Restart(d, now + 0.3f);
                        }
                    }
                }
                else if (now - d.blowAt > (Moves ? 3f : 1.5f))
                {
                    d.wantsIn = false;
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
                case Gesture.Dodge:
                    if (!clip.Dodge(dice.Next(2) == 0 ? -1 : 1))
                    {
                        return false;
                    }

                    furthest = PostureMaker.DodgeDown;
                    lasts = PostureMaker.DodgeDown + PostureMaker.DodgeHeld + PostureMaker.DodgeUp;
                    break;
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
            d.runOpen = false;
            d.Say((tell ? "HIS TELL: " : "") + Name(kind), now, Mathf.Max(0.8f, lasts));
            Sees(d.Foe, d.Now, now);
            return true;
        }

        // ---- Counted -------------------------------------------------------------------------------------------------

        public int Gestures(Dancer d) => d.Gestures.Count;

        /// <summary>One fighter's blows by class so far: how many, how long each took to land, how many landed and what those did.</summary>
        public string Classes(Dancer d)
        {
            var parts = new List<string>();
            for (int k = 0; k < 3; k++)
            {
                if (d.ClassThrown[k] == 0)
                {
                    continue;
                }

                string did = d.ClassLanded[k] > 0 ? string.Format(System.Globalization.CultureInfo.InvariantCulture, ", {0} landed at {1:0.0} m/s doing {2:0.0} harm each", d.ClassLanded[k], d.ClassSpeed[k] / d.ClassLanded[k], d.ClassHarm[k] / d.ClassLanded[k]) : ", none landed";
                parts.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1} (to land in {2:0.00} s{3})", d.ClassThrown[k], AttackClasses.Name((AttackClass)k), d.ClassLandsIn[k] / d.ClassThrown[k], did));
            }

            return (parts.Count > 0 ? string.Join("; ", parts) : "no blows") + $"; struck into the other's wind-up {d.StopHits} times; dodged {d.DodgedBlows} blows";
        }

        /// <summary>The gap and the two's feet so far, counted.</summary>
        public string Feet()
        {
            if (GapSteps == 0)
            {
                return "";
            }

            string Of(Dancer d)
            {
                var parts = new List<string>();
                float all = 0f;
                foreach (var kv in d.TempoTime)
                {
                    all += kv.Value;
                }

                foreach (var kv in d.TempoTime)
                {
                    parts.Add($"{kv.Key} {100f * kv.Value / Mathf.Max(0.01f, all):0}%");
                }

                return $"walked {d.Walked:0.0} m, leapt in {d.LeaptIn} and out {d.LeaptOut} times" + (parts.Count > 0 ? " (" + string.Join(", ", parts) + ")" : "");
            }

            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "the gap {0:0.00} m on average, {1:0.00} to {2:0.00} m; him: {3}; the other: {4}", GapSum / GapSteps, GapLeast, GapMost, Of(A), Of(B));
        }

        /// <summary>One fighter's dance so far, counted.</summary>
        public string Tally(Dancer d)
        {
            int tells = 0;
            foreach (var g in d.Gestures)
            {
                tells += g.Tell ? 1 : 0;
            }

            int blows = d.Landed + d.Stopped + d.Missed;
            var where = new List<string>();
            foreach (var kv in d.LandedOn)
            {
                where.Add(kv.Value + " " + kv.Key);
            }

            string landedOn = where.Count > 0 ? " (" + string.Join(", ", where) + $"; {d.OnSpot} on the part he went for)" : "";
            return $"{d.Gestures.Count} gestures ({tells} tells), {blows} blows: {d.Landed} landed{landedOn}, {d.Stopped} stopped, {d.Missed} met nothing ({d.ReadBlows} of them thrown by what he read, {d.Answered} in answer to a blow he stopped); "
                + $"took {d.Taken} ({d.CaughtInFake} while in a fake of his own, {d.TakenAnswers} in answer to a blow of his that was stopped); expected {d.ExpectedBlows} of the other's pattern blows; covered for nothing {d.CoveredForNothing} times; the fight goes {d.Going(lastNow):+0;-0;0} for him";
        }
    }
}
