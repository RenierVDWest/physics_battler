using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>
    /// The footwork's meter (the plan "Convincing walking in the dance", step 1): what one fighter's walking and
    /// standing looked like over a stretch, in a few numbers that say where a body jumps. The band his hips' height
    /// moved over (the middle 90% of the time); how often his hips rose or sank 10 cm or more within 0.3 s; how many
    /// steps he took in each spell of walking; how much of his walking was two recordings mixed, each a quarter or
    /// more; and how fast his steps were against the recordings' own. Only while he stands or walks: a gesture, a
    /// blow or a leap is not counted. Heights are at a Medium's size.
    /// </summary>
    public sealed class FootworkMeter
    {
        private readonly System.Collections.Generic.List<float> heights = new System.Collections.Generic.List<float>(4096);
        private readonly float[] recent = new float[64];
        private readonly bool[] recentCounts = new bool[64];
        private int recentAt, steps, spells, walking, mixed, jumps;
        private float sinceJump, lastPhase, tempoSum, time;
        private bool wasWalking, hadPhase;
        private float[] sorted = new float[0];

        public float Seconds => time;
        public int Jumps => jumps;
        public int Spells => spells;
        public int Steps => steps;

        /// <summary>The hips' band (metres): the middle 90% of their heights. Its low and high ends in <paramref name="low"/> and <paramref name="high"/>.</summary>
        public float Band(out float low, out float high)
        {
            low = high = 0f;
            if (heights.Count < 20)
            {
                return 0f;
            }

            if (sorted.Length < heights.Count)
            {
                sorted = new float[heights.Capacity];
            }

            heights.CopyTo(sorted);
            System.Array.Sort(sorted, 0, heights.Count);
            low = sorted[Mathf.RoundToInt(0.05f * (heights.Count - 1))];
            high = sorted[Mathf.RoundToInt(0.95f * (heights.Count - 1))];
            return high - low;
        }

        /// <summary>How much of his walking was two recordings mixed, each a quarter or more (0 to 1).</summary>
        public float Mixed => walking > 0 ? (float)mixed / walking : 0f;

        /// <summary>Steps taken in each spell of walking, on average.</summary>
        public float StepsASpell => spells > 0 ? (float)steps / spells : 0f;

        /// <summary>How fast his steps went against the recordings' own, on average over his walking (1: as recorded).</summary>
        public float Tempo => walking > 0 ? tempoSum / walking : 0f;

        public void Reset()
        {
            heights.Clear();
            recentAt = steps = spells = walking = mixed = jumps = 0;
            sinceJump = 10f;
            tempoSum = time = 0f;
            wasWalking = hadPhase = false;
            for (int i = 0; i < recentCounts.Length; i++)
            {
                recentCounts[i] = false;
            }
        }

        /// <summary>A step of the game: the fighter as he is posed now.</summary>
        public void Step(UnitController unit, float dt)
        {
            if (unit == null || unit.Clip == null || unit.Clip.Loco == null || unit.Clip.Anim == null || dt <= 0f)
            {
                return;
            }

            var loco = unit.Clip.Loco;
            time += dt;
            sinceJump += dt;
            bool counts = loco.Current != PhysicsBattler.Locomotion.Locomotor.Mode.OneShot && !unit.IsDown && !loco.Dipping;
            float size = Mathf.Max(0.1f, unit.Body.SizeFactor);
            float height = unit.Clip.Anim.Part(PhysicsBattler.Rig.PartId.Pelvis).position.y / size;
            int span = Mathf.Clamp(Mathf.RoundToInt(0.3f / dt), 2, recent.Length - 1);
            recent[recentAt % recent.Length] = height;
            recentCounts[recentAt % recent.Length] = counts;
            recentAt++;
            if (counts)
            {
                if (heights.Count < 200000)
                {
                    heights.Add(height);
                }

                // (A rise or sink of 10 cm within 0.3 s, with nothing but standing and walking in between.)
                bool whole = recentAt > span;
                for (int k = 0; k <= span && whole; k++)
                {
                    whole &= recentCounts[(recentAt - 1 - k) % recent.Length];
                }

                if (whole && sinceJump > 0.6f && Mathf.Abs(height - recent[(recentAt - 1 - span) % recent.Length]) >= 0.1f)
                {
                    jumps++;
                    sinceJump = 0f;
                }
            }

            bool walks = loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.Cycle;
            if (walks)
            {
                walking++;
                spells += wasWalking ? 0 : 1;
                // (A foot lands as the step's phase passes 0 or a half.)
                float phase = loco.Phase;
                if (hadPhase && wasWalking && ((lastPhase < 0.5f && phase >= 0.5f) || (phase < lastPhase && lastPhase > 0.75f)))
                {
                    steps++;
                }

                lastPhase = phase;
                hadPhase = true;
                int big = 0;
                float total = 0f;
                foreach (var (_, weight) in loco.Blend)
                {
                    total += weight;
                }

                foreach (var (_, weight) in loco.Blend)
                {
                    big += total > 1e-4f && weight / total >= 0.25f ? 1 : 0;
                }

                mixed += big >= 2 ? 1 : 0;
                tempoSum += loco.Cadence;
            }

            wasWalking = walks;
        }

        /// <summary>The meter in words.</summary>
        public string Text
        {
            get
            {
                if (time < 0.5f)
                {
                    return "nothing measured yet.";
                }

                float band = Band(out float low, out float high);
                return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "over {0:0} s: his hips' height moved over a band of {1:0} cm ({2:0} to {3:0} cm); they rose or sank 10 cm or more within 0.3 s {4} times; he walked in {5} spells, {6:0.0} steps a spell; two recordings mixed for {7:0}% of his walking; his steps went at {8:0.00} of the recordings' own tempo",
                    time, 100f * band, 100f * low, 100f * high, jumps, spells, StepsASpell, 100f * Mixed, Tempo);
            }
        }
    }

    /// <summary>
    /// The duel lab's movement orders (the plan "Fight Rhythm from Recordings", step 1): one fighter moved about
    /// another on demand, so a way of moving can be looked at by itself. Walk to it, back away from it, go round it
    /// to the left or the right, stand. The fighter is steered as the fight steers one (a velocity and a facing, to
    /// its walk), so what is seen is the walk the fight will have: planted feet, real steps.
    /// </summary>
    public sealed class DuelLab
    {
        public enum Order
        {
            Stand,
            WalkTo,
            BackAway,
            CircleLeft,
            CircleRight,
            LeapIn,
            LeapOut,
            HopLeft,
            HopRight,
        }

        /// <summary>How far a leap goes (metres at a Medium's size) and how long it takes (seconds).</summary>
        public float LeapFar = 0.5f, LeapTime = 0.25f;

        /// <summary>How the last leap went: asked, and whether both feet left the ground together (a hop) or stepped after the body.</summary>
        public string LastLeap = "none yet";

        /// <summary>The order it is carrying out.</summary>
        public Order Now { get; private set; }

        /// <summary>How near it walks to the other, and how far it backs away (metres, root to root).</summary>
        public float Near = 1.5f, Far = 4f;

        /// <summary>
        /// The pace he is asked to go at (m/s at a Medium's size; 0: his guard's own walking pace, as the fight asked
        /// until now). Under a walk's speed he stands and steps; at a walk's he walks; above it his guard's run
        /// carries him, its stride shortened to the pace.
        /// </summary>
        public float Pace;

        /// <summary>The pace he goes at now (m/s): the one asked, or his walk's own.</summary>
        public float PaceNow => Mover == null ? 0f : Pace > 0.01f ? Pace * Mathf.Sqrt(Mathf.Max(0.1f, Mover.Body.SizeFactor)) : Mover.WalkSpeed;

        /// <summary>The fastest he went under the order he has (m/s, over the ground).</summary>
        public float Fastest { get; private set; }

        /// <summary>
        /// Setting off: over the first 0.35 s of the order he has, the fastest either foot went and the fastest his
        /// hips went (m/s, the body as seen), and how far he had got by then (metres). Feet whipped to their places
        /// while the body has hardly moved read as a jump.
        /// </summary>
        public float SetOffFoot { get; private set; }
        public float SetOffHips { get; private set; }
        public float SetOffWent { get; private set; }

        /// <summary>Set (the headless run): each step of a setting off is written to it.</summary>
        public System.Collections.Generic.List<string> Trace;

        private bool leaping;
        private float since;
        private Vector3 lastFootL, lastFootR, lastHips;

        /// <summary>
        /// The movement style the mover is given (the plan's step 4; null: the one he has): the standard style, or the
        /// capoeira style, which stands in the ginga. Given as he is found without it, and again when what the style
        /// is made of changes (the ginga's size, its take).
        /// </summary>
        public PhysicsBattler.Locomotion.MoveStyle Style;

        private string styleGiven = "";

        private void GiveStyle()
        {
            if (Style == null || Mover.Clip == null)
            {
                return;
            }

            string now = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1} {2:0.00} {3:0.00} {4} {5:0.00} {6:0.00} {7}", Style.Name, PhysicsBattler.Locomotion.MoveStyle.GingaTake,
                PhysicsBattler.Locomotion.MoveStyle.StandSize, PhysicsBattler.Locomotion.MoveStyle.StandArms,
                PhysicsBattler.Locomotion.MoveStyle.GuardWalkRebuilt, PhysicsBattler.Locomotion.MoveStyle.RebuiltCarriage, PhysicsBattler.Locomotion.MoveStyle.RebuiltMotion, PhysicsBattler.Locomotion.StandPattern.Generated);
            if (Mover.Clip.Loco.Style != Style || now != styleGiven)
            {
                Mover.Clip.Loco.SetStyle(Style);
                styleGiven = now;
            }

            if (Style.StandSteps)
            {
                // (A style with a stand of its own is stood in with empty hands too, where a fighter with nothing to guard with would stand at ease.)
                Mover.Clip.Loco.GuardWanted = true;
            }
        }

        /// <summary>The fighter moved, and the one it moves about.</summary>
        public UnitController Mover { get; private set; }
        public UnitController Other { get; private set; }

        /// <summary>How far apart the two are now (metres, root to root), how fast the mover is going over the ground (m/s) and how far it has gone since its order.</summary>
        public float Gap { get; private set; }
        public float Speed { get; private set; }
        public float Gone { get; private set; }

        private float radius;
        private Vector3 last;

        public bool Running => Mover != null && Other != null && Mover.IsInitialized && Other.IsInitialized;

        // ---- The footwork course (the plan "Convincing walking in the dance", step 1) ----------------------------------

        /// <summary>The mover's footwork, measured while the course runs.</summary>
        public readonly FootworkMeter Meter = new FootworkMeter();

        /// <summary>The course is being run.</summary>
        public bool CourseOn { get; private set; }

        /// <summary>The course: the same footwork every time, at the dance's pace and distances. Each leg an order and how long it is kept (seconds).</summary>
        public static readonly (Order order, float seconds, string about)[] CourseLegs =
        {
            (Order.Stand, 0.8f, "stands"),
            (Order.BackAway, 2.4f, "retreats"),
            (Order.WalkTo, 2.4f, "advances"),
            (Order.CircleLeft, 3f, "circles left"),
            (Order.CircleRight, 3f, "circles right"),
            (Order.WalkTo, 1.1f, "in"),
            (Order.BackAway, 1.1f, "out"),
            (Order.WalkTo, 1.1f, "in"),
            (Order.BackAway, 1.1f, "out"),
            (Order.Stand, 0.6f, "stops"),
            (Order.CircleLeft, 0.8f, "goes left"),
            (Order.Stand, 0.6f, "stops"),
            (Order.CircleRight, 0.8f, "goes right"),
            (Order.Stand, 0.6f, "stops"),
            (Order.WalkTo, 0.8f, "goes in"),
            (Order.Stand, 0.6f, "stops"),
            (Order.BackAway, 0.8f, "goes out"),
            (Order.Stand, 1f, "stands"),
        };

        /// <summary>The course's pace (m/s at a Medium's size) and the distances it walks to and backs to (metres): the dance's.</summary>
        public const float CoursePace = 0.8f, CourseNear = 1.5f, CourseFar = 2.5f;

        /// <summary>How long the whole course takes (seconds).</summary>
        public static float CourseSeconds
        {
            get
            {
                float all = 0f;
                foreach (var leg in CourseLegs)
                {
                    all += leg.seconds;
                }

                return all;
            }
        }

        private int courseLeg;
        private float courseFor, courseWasPace, courseWasNear, courseWasFar;

        /// <summary>What the course is at, in words.</summary>
        public string CourseText => CourseOn ? $"leg {courseLeg + 1} of {CourseLegs.Length}: he {CourseLegs[courseLeg].about}" : Meter.Seconds > 0.5f ? "done" : "not run yet";

        /// <summary>The course is begun: the mover is taken through its legs, and his footwork measured from its first step.</summary>
        public bool BeginCourse()
        {
            if (!Running)
            {
                return false;
            }

            if (!CourseOn)
            {
                courseWasPace = Pace;
                courseWasNear = Near;
                courseWasFar = Far;
            }

            Pace = CoursePace;
            Near = CourseNear;
            Far = CourseFar;
            CourseOn = true;
            courseLeg = 0;
            courseFor = 0f;
            Meter.Reset();
            Give(CourseLegs[0].order);
            return true;
        }

        /// <summary>The course is ended, by its last leg or before it: he stands, at the pace and distances he had.</summary>
        public void EndCourse()
        {
            if (!CourseOn)
            {
                return;
            }

            CourseOn = false;
            Pace = courseWasPace;
            Near = courseWasNear;
            Far = courseWasFar;
            if (Running)
            {
                Give(Order.Stand);
            }
        }

        private void StepCourse(float dt)
        {
            if (!CourseOn)
            {
                return;
            }

            Meter.Step(Mover, dt);
            courseFor += dt;
            if (courseFor < CourseLegs[courseLeg].seconds)
            {
                return;
            }

            courseFor = 0f;
            courseLeg++;
            if (courseLeg >= CourseLegs.Length)
            {
                courseLeg = CourseLegs.Length - 1;
                EndCourse();
                // (Into the playtest's log, so a run of the course in Unity can be read afterwards: with whole steps
                // on or off, and the legs turned or mixed.)
                PhysicsBattler.Diagnostics.PlaytestTrace.Note(Mover, "course", $"the footwork course ({(PhysicsBattler.Locomotion.Locomotor.WholeSteps ? "whole steps" : "steps as asked")}, a diagonal {(PhysicsBattler.Locomotion.Locomotor.LegsTurned ? "turns the legs" : "is mixed")}): {Meter.Text}");
                return;
            }

            Give(CourseLegs[courseLeg].order);
        }

        public void Begin(UnitController mover, UnitController other)
        {
            Mover = mover;
            Other = other;
            Now = Order.Stand;
            Gone = 0f;
            StoodOff = 0f;
            learntAt = new Vector3(float.MaxValue, 0f, 0f);
            wasGoing = takesDistance = false;
            TookDistance = "he has not gone for him yet";
            if (mover != null)
            {
                last = mover.Root.position;
            }
        }

        public void Stop()
        {
            CourseOn = false;
            if (Mover != null)
            {
                Mover.DummyGo = null;
            }

            Mover = Other = null;
            Now = Order.Stand;
        }

        /// <summary>Gives the mover an order. Going round, it keeps the distance it has now.</summary>
        public void Give(Order order)
        {
            if (!Running)
            {
                return;
            }

            Now = order;
            // (An order of his own: where it takes him is where he stands.)
            takesDistance = false;
            Gone = 0f;
            Fastest = 0f;
            since = 0f;
            SetOffFoot = SetOffHips = SetOffWent = 0f;
            lastFootL = Mover.PartPosition(PhysicsBattler.Rig.PartId.FootL);
            lastFootR = Mover.PartPosition(PhysicsBattler.Rig.PartId.FootR);
            lastHips = Mover.PartPosition(PhysicsBattler.Rig.PartId.Pelvis);
            radius = Flat(Other.Root.position - Mover.Root.position).magnitude;
            if (order == Order.Stand)
            {
                Stand();
            }
            else if (order >= Order.LeapIn)
            {
                // (A leap is done at once, from where he stands; then he stands where it takes him.)
                Vector3 toward = Flat(Other.Root.position - Mover.Root.position);
                toward = toward.sqrMagnitude > 1e-6f ? toward.normalized : Mover.transform.forward;
                Vector3 left = Vector3.Cross(toward, Vector3.up);
                Vector3 way = order == Order.LeapIn ? toward : order == Order.LeapOut ? -toward : order == Order.HopLeft ? left : -left;
                Mover.DummyGo = null;
                bool made = PhysicsBattler.Locomotion.LeapMaker.On;
                bool leapt = Mover.Clip != null && (made ? Mover.Clip.LeapMade(way, LeapFar, LeapTime) : Mover.Clip.LeapNow(way, LeapFar, LeapTime));
                Mover.DummySpot = Mover.Root.position + (leapt ? way * (LeapFar * Mover.Body.SizeFactor) : Vector3.zero);
                LastLeap = !leapt ? $"{order}: he could not (in a blow, or down)" : $"{order}: {LeapFar:0.00} m, {LeapTime:0.00} s in the air, " + (made ? "made of the recorded jump" : Mover.Clip.LeapHopped ? "the stance's hop: both feet off the ground together" : "stepped after the body (he was not standing in his stance)");
                leaping = true;
            }
        }

        // ---- A blow asked for while he moves (step 3: attacks from any pose) ----

        /// <summary>What he was doing as the last blow was asked for, and how its start went (the first 0.4 s; "" until it is over).</summary>
        public string BlowBegan { get; private set; } = "";
        public string BlowStart { get; private set; } = "";

        /// <summary>How the last blow was thrown: on the move (the walk going on under it, and the step in it took), or planted (and why, when it was asked for on the move).</summary>
        public string BlowThrown { get; private set; } = "";

        private static readonly PhysicsBattler.Rig.PartId[] watched = { PhysicsBattler.Rig.PartId.Pelvis, PhysicsBattler.Rig.PartId.FootL, PhysicsBattler.Rig.PartId.FootR, PhysicsBattler.Rig.PartId.HandR };
        private readonly Vector3[] watchAt = new Vector3[4], watchSpeed = new Vector3[4];
        private readonly float[] watchJump = new float[4], watchFirst = new float[4];
        private Vector3 watchRoot, watchRootSpeed, watchFrom;
        private float watchFor = -1f, watchRootJump, watchRootFirst, watchWas, watchSlid;
        private int watchSteps;
        private bool watchKnown;
        private float watchLowest = float.MaxValue;

        /// <summary>How long a blow's start is watched (seconds).</summary>
        public const float StartWatched = 0.4f;

        /// <summary>
        /// One blow at a spot on the other, asked for now, whatever he is doing: in the middle of a walk, going round,
        /// backing away. The order he has goes on after it. How its start goes is watched: how much the speed of his
        /// root and, as posed, of his hips, each foot and his sword hand changes in the blow's first step (a body going
        /// 1.4 m/s that stops in one step shows as 1.4: the snap), the biggest such change in any one step of its
        /// first 0.4 s, and how far his held feet slide.
        /// </summary>
        public bool Strike(AimTest aim, PhysicsBattler.Locomotion.AimSpot spot, float now)
        {
            if (!Running || aim == null || Mover.Clip == null)
            {
                return false;
            }

            var loco = Mover.Clip.Loco;
            BlowBegan = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, going {1:0.00} m/s, {2}, feet down {3:0}/{4:0}, {5:0.00} m apart",
                Now, Speed, loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.Idle && Speed > 0.2f ? "standing and stepping" : loco.Current.ToString(), loco.Down[0], loco.Down[1], Gap);
            BlowStart = "";
            if (!aim.Order(spot, now))
            {
                watchFor = -1f;
                return false;
            }

            var made = Mover.Clip.Made;
            string posture = made != null && made.Posture ? (made.Feint > 0f && made.FakeLunge > 0f ? "a feint on a fake lunge" : made.Feint > 0f ? "a feint" : "a fake lunge") + string.Format(System.Globalization.CultureInfo.InvariantCulture, ", furthest {0:0.00} s after, over in {1:0.00} s: ", made.ContactAt, made.Clip.Duration) : "";
            BlowThrown = posture + (loco.ShotOverWalk && made != null
                ? (Mover.Clip.MovedShort > 0f ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "out of reach, {0:0} cm short: ", 100f * Mover.Clip.MovedShort) : "") + string.Format(System.Globalization.CultureInfo.InvariantCulture, "thrown on the move ({0}, to land {1:0.00} s after, {2})", made.Clip.Name.Replace("made_sns_", ""), made.LandsIn,
                    made.StepIn.magnitude * loco.Leg < 0.03f ? "no step in" : string.Format(System.Globalization.CultureInfo.InvariantCulture, "a step {0} of {1:0} cm", Vector3.Dot(made.StepIn, made.Asked.Target) >= 0f ? "in" : "back", 100f * made.StepIn.magnitude * loco.Leg))
                : Mover.Clip.MovedShort > 0f
                    ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "planted (on the move it would have fallen {0:0} cm short)", 100f * Mover.Clip.MovedShort)
                    : PhysicsBattler.Locomotion.AttackOnTheMove.On && Speed < PhysicsBattler.Locomotion.AttackOnTheMove.WalksFrom ? "planted (he was not walking)" : "planted");
            watchFrom = loco.Position;
            watchWas = watchRootSpeed.magnitude;
            watchFor = 0f;
            watchRootJump = watchSlid = 0f;
            watchSteps = 0;
            for (int i = 0; i < watched.Length; i++)
            {
                watchJump[i] = 0f;
            }

            return true;
        }

        // ---- Two blows (the combos plan's step 2) ----

        /// <summary>The two strikes "Throw two" throws: a forehand down, and a rising backhand back up the same line (the figure of eight).</summary>
        public const string TwoFirst = "sns_forehand", TwoNext = "sns_rising";

        /// <summary>For the reports: the spot the second of the two is thrown at (null: the same as the first; <c>FIGHT_TWOSECOND=Head</c>).</summary>
        public static PhysicsBattler.Locomotion.AimSpot? TwoSecondSpot;

        /// <summary>For the panel and the reports: what the last two blows asked for together did.</summary>
        public string TwoTold { get; private set; } = "none thrown yet";

        private AimTest twoAim;
        private PhysicsBattler.Locomotion.AimSpot twoSpot;
        private int twoStage, twoLandings;
        private float twoFirstHit, twoFor;
        private string twoFit = "";

        /// <summary>
        /// Two blows at the spot: a forehand, then a rising backhand. Flowing (<see cref="PhysicsBattler.Locomotion.ClipFighter.BlowsFlow"/>):
        /// as one swing, the second begun out of the first's carry-through. Otherwise the old way: one whole attack,
        /// and when he is free again the other. False: the first could not be thrown.
        /// </summary>
        public bool ThrowTwo(AimTest aim, PhysicsBattler.Locomotion.AimSpot spot, float now)
        {
            if (!Running || aim == null || Mover.Clip == null || Other.Clip == null || Mover.Clip.Flowing || twoStage != 0)
            {
                return false;
            }

            string was = aim.Attack;
            aim.Attack = TwoFirst;
            bool begun = Strike(aim, spot, now);
            aim.Attack = was;
            if (!begun)
            {
                return false;
            }

            twoAim = aim;
            twoSpot = spot;
            twoFor = 0f;
            twoFirstHit = -1f;
            twoLandings = Landings();
            var other = Other;
            var second = TwoSecondSpot ?? spot;
            if (PhysicsBattler.Locomotion.ClipFighter.BlowsFlow && Mover.Clip.FlowInto(Other.Clip, () => PhysicsBattler.Locomotion.AimSpots.Point(other.Clip, second), PhysicsBattler.Locomotion.AttackClasses.StyleOf(aim.Kind), TwoNext))
            {
                twoStage = 3;
                TwoTold = "flowing: under way";
            }
            else
            {
                twoStage = 1;
                TwoTold = "one after the other (old): under way";
            }

            return true;
        }

        private int Landings() => Mover.Clip.Strikes != null ? Mover.Clip.Strikes.Hits + Mover.Clip.Strikes.Blocks + Mover.Clip.Strikes.Parries + Mover.Clip.Strikes.Deflects : 0;

        private void StepTwo(float dt)
        {
            if (twoStage == 0 || Mover == null || Mover.Clip == null)
            {
                twoStage = 0;
                return;
            }

            twoFor += dt;
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            if (twoStage == 3)
            {
                // (Flowing: the fighter's own account, as it ends.)
                if (!Mover.Clip.Flowing)
                {
                    TwoTold = "flowing: " + Mover.Clip.FlowTold;
                    twoStage = 0;
                }

                return;
            }

            int landings = Landings();
            bool landed = landings != twoLandings;
            twoLandings = landings;
            if (twoStage == 1)
            {
                if (landed && twoFirstHit < 0f)
                {
                    twoFirstHit = twoFor;
                }

                // (The first is over and he is free: the other.)
                if (!Mover.Clip.Striking && !twoAim.Following && twoFor > 0.3f)
                {
                    string was = twoAim.Attack;
                    twoAim.Attack = TwoNext;
                    bool begun = Strike(twoAim, twoSpot, Time.time);
                    twoAim.Attack = was;
                    if (begun)
                    {
                        twoStage = 2;
                        var m = Mover.Clip.Made;
                        twoFit = m == null ? "" : string.Format(inv, " [the second as made: turned {0:0} degrees, lunge x{1:0.00}, hips dropped {2:0} cm, leaned {3:0} degrees, hand moved {4:0} cm, wrist steered {5:0} degrees; lands {6:0.00} s in, at {7:0.00} s of its recording]",
                            m.Turn, m.Lunge, 100f * m.Drop * Mover.Clip.Loco.Leg, m.Lean, 100f * m.HandMoved * Mover.Clip.Loco.Leg, m.Steer, m.ContactAt, m.ScoreTime);
                    }
                    else if (twoFor > 6f)
                    {
                        TwoTold = "one after the other (old): the second could not be thrown";
                        twoStage = 0;
                    }
                }
            }
            else if (landed || twoFor > 8f || (!Mover.Clip.Striking && twoFor > 0.5f && !twoAim.Following))
            {
                TwoTold = landed && twoFirstHit >= 0f
                    ? string.Format(inv, "one after the other (old): the second landed {0:0.00} s after the first ({1})", twoFor - twoFirstHit, Mover.Clip.Strikes.Last) + twoFit
                    : "one after the other (old): " + (twoFirstHit < 0f ? "the first met nothing" : "the second met nothing");
                PhysicsBattler.Diagnostics.PlaytestTrace.Note(Mover, "flow", TwoTold);
                twoStage = 0;
            }
        }

        /// <summary>Each step: how fast his root and the watched parts go (as posed), and through a blow's start how that changes.</summary>
        private void WatchStart(float dt)
        {
            if (Mover.Clip == null || dt <= 1e-5f)
            {
                watchKnown = false;
                return;
            }

            var loco = Mover.Clip.Loco;
            bool watching = watchFor >= 0f && watchKnown;
            Vector3 v = watchKnown ? (loco.Position - watchRoot) / dt : loco.Velocity;
            watchRoot = loco.Position;
            if (watching)
            {
                float jump = (v - watchRootSpeed).magnitude;
                watchRootJump = Mathf.Max(watchRootJump, jump);
                watchRootFirst = watchSteps == 0 ? jump : watchRootFirst;
            }

            watchRootSpeed = v;
            for (int i = 0; i < watched.Length; i++)
            {
                Vector3 p = Mover.Clip.Anim.Part(watched[i]).position, speed = watchKnown ? (p - watchAt[i]) / dt : Vector3.zero;
                if (watching)
                {
                    float jump = (speed - watchSpeed[i]).magnitude;
                    watchJump[i] = Mathf.Max(watchJump[i], jump);
                    watchFirst[i] = watchSteps == 0 ? jump : watchFirst[i];
                    // (A foot's slide: how far it goes over the ground while it is held down and flat. With its heel
                    // up its ankle goes round over its toe, which is not a slide.)
                    if ((i == 1 || i == 2) && Mover.Clip.Poser.Feet[i - 1].Locked && !Mover.Clip.Poser.Feet[i - 1].OnToe && p.y <= watchLowest + 0.015f)
                    {
                        watchSlid += Flat(p - watchAt[i]).magnitude;
                    }
                }

                if (i == 1 || i == 2)
                {
                    // (An ankle's standing height: the lowest one has been.)
                    watchLowest = Mathf.Min(watchLowest, p.y);
                }

                watchAt[i] = p;
                watchSpeed[i] = speed;
            }

            watchKnown = true;
            if (!watching)
            {
                return;
            }

            watchSteps++;
            watchFor += dt;
            if (watchFor >= StartWatched)
            {
                watchFor = -1f;
                BlowStart = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "he was going {0:0.00} m/s and went {1:0.00} m on in the blow's first {2:0.0} s; in its first step the speed of his root changed by {3:0.00} m/s, his hips {4:0.00}, left foot {5:0.00}, right foot {6:0.00}, sword hand {7:0.00}; the biggest change in any one step: root {8:0.00}, hips {9:0.00}, left foot {10:0.00}, right foot {11:0.00}, sword hand {12:0.00}; his held feet slid {13:0.0} cm",
                    watchWas, Flat(loco.Position - watchFrom).magnitude, StartWatched, watchRootFirst, watchFirst[0], watchFirst[1], watchFirst[2], watchFirst[3], watchRootJump, watchJump[0], watchJump[1], watchJump[2], watchJump[3], 100f * watchSlid);
            }
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        /// <summary>
        /// The distance he keeps is learnt while both stand settled on their spots; after a going of his own he is
        /// given a spot that far from the other's, on his own side of it, once, and again only if the other's spot
        /// moves (a man who fell stands where he got up).
        /// </summary>
        private void StepDistance()
        {
            if (Mover == null || Other == null || Mover.Clip == null || Other.Clip == null || Now != Order.Stand || leaping)
            {
                return;
            }

            float size = Mover.Clip.Size;
            Vector3 his = Flat(Other.DummySpot);
            if (!takesDistance && TakesDistanceAgain && StoodOff > 0f && learntAt.x < 1e30f && (his - learntAt).magnitude > 0.2f * size
                && !Other.Clip.Fallen && !Other.Clip.Stumbling && !Run.Going)
            {
                // (The other has been driven off its spot and has come to rest somewhere else, a stumble's end or where
                // it got up: he takes his distance to it again, as after a going of his own.)
                takesDistance = true;
                takenFrom = new Vector3(float.MaxValue, 0f, 0f);
                learntAt = his;
                TookDistance = "";
            }

            if (!takesDistance)
            {
                bool settled = !Other.Clip.Fallen && !Other.Clip.Stumbling && !Mover.Clip.Stumbling && !Mover.Clip.Striking
                    && Flat(Mover.Root.position - Mover.DummySpot).magnitude < SettledWithin * size && Flat(Other.Root.position - Other.DummySpot).magnitude < SettledWithin * size;
                if (settled)
                {
                    StoodOff = Flat(Mover.DummySpot - Other.DummySpot).magnitude;
                    learntAt = his;
                }

                return;
            }

            // (Not while his blow is still playing, and not while he is finishing the step his run stopped in: a man
            // told to stand in the middle of a step stands where the step ends, FightLoop's rule of 18 November.)
            if ((his - takenFrom).magnitude < 0.1f * size || Mover.Clip.Stumbling || Mover.Clip.Fallen || Mover.Clip.Loco == null || Mover.Clip.Loco.StepUnderWay)
            {
                return;
            }

            // (Not while he is still running on from his going: the way back is counted from where he comes to rest.
            // Set as his run was called off at 2 m/s, it was counted from a place he then ran a metre past, and his walk
            // back took him by the other man at arm's length: Renier's log of 20 November, 22:34.)
            if (takenFrom.x > 1e30f && Speed > ComesToRestUnder * size)
            {
                return;
            }

            if (Mover.Clip.Striking)
            {
                // (His blow has landed and been carried through: he does not stand through its way back to the guard,
                // with the other walking back into him. Renier's log of 20 November, 21:18: about a second of that.)
                var made = Mover.Clip.Made;
                var loco = Mover.Clip.Loco;
                if (StepsBackAfter <= 0f || made == null || loco.Shot != made.Clip || loco.Rewinding || Mover.Clip.Flowing || loco.ShotTime < made.ContactAt + StepsBackAfter)
                {
                    return;
                }

                loco.EndShotNow();
            }

            // (Straight back from the other's spot through where he is: the shortest way, and never through the man.)
            Vector3 from = Flat(Mover.Root.position) - his;
            float was = from.magnitude;
            Vector3 way = was > 0.05f ? from / was : -Flat(Mover.transform.forward).normalized;
            Vector3 spot = his + way * StoodOff;
            Mover.DummyGo = null;
            Mover.DummySpot = new Vector3(spot.x, Mover.Root.position.y, spot.z);
            if (takenFrom.x > 1e30f)
            {
                TookDistance = string.Format(System.Globalization.CultureInfo.InvariantCulture, "it ended {0:0.00} m from the other's spot; he steps {1} to the {2:0.00} m he stood at", was, was < StoodOff ? "back" : "in", StoodOff);
            }

            takenFrom = his;
            learntAt = his;
        }

        private void Stand()
        {
            // (Where it is now is its spot: it does not walk back to where it began.)
            Now = Order.Stand;
            Mover.DummyGo = null;
            Mover.DummySpot = Mover.Root.position;
        }

        /// <summary>One step of the lab (the physics step): the order turned into the velocity the mover is steered by.</summary>
        // ---- Going for him: in a class of its own, so the dance's two fighters each have one (20 November) ----

        /// <summary>The mover's running in and striking: the pounce on an opening, and the blow at a man who is down.</summary>
        public readonly RunIn Run = new RunIn();

        /// <summary>
        /// After he has gone for a man by himself (a pounce on an opening, a blow at a man who is down) and he has no
        /// order but to stand, he takes his distance again: he steps back to as far from the other's spot as he stood
        /// before he went (Combat Feel "After Going For Him He Takes His Distance Again"; <c>FIGHT_TAKESDISTANCE=0</c>
        /// for the old way). Renier, 20 November, of his first look at the blade's path: "After the first time I attack
        /// it seems like the blue dummy keeps walking into me. So for all the attacks after that the dummy was standing
        /// right against me." His log: the swordsman pounced on the stumbling target, 0.66 m in, and stood where the
        /// pounce ended, as the lab's mover always has ("where it is now is its spot"); the target walked back to its
        /// own spot, which was now 0.70 m from him, and every blow after that was thrown from 0.55 to 0.86 m, not the
        /// 1.5 m the lab sets out. Old: he stands where the going ended.
        /// </summary>
        public static bool TakesDistanceAgain = true;

        /// <summary>
        /// He begins his step back this long after his blow landed, in the blow's own time (seconds): the rest of the
        /// blow, its way back to the guard, is left. 0: he waits for the whole blow, as on 20 November at 21:15
        /// (<c>FIGHT_STEPSBACKAFTER</c>).
        /// </summary>
        public static float StepsBackAfter = 0.35f;

        /// <summary>His way back is counted once he goes slower than this after his going (m/s at a Medium's size).</summary>
        public const float ComesToRestUnder = 0.8f;

        /// <summary>For the panel and the reports: how far off he stood (spot to spot) as it was last seen settled, and what he did after his last going.</summary>
        public float StoodOff { get; private set; }
        public string TookDistance { get; private set; } = "he has not gone for him yet";

        /// <summary>Both count as standing on their spots within this (metres at a Medium's size): the distance between the spots is then the one he keeps.</summary>
        public const float SettledWithin = 0.15f;

        private bool wasGoing, takesDistance;
        private Vector3 takenFrom, learntAt = new Vector3(float.MaxValue, 0f, 0f);

        public bool Going => Run.Going;
        public string GoingNow => Run.GoingNow;
        public string Pounced => Run.Pounced;
        public int OpeningsSeen => Run.OpeningsSeen;
        public int PounceLeaps => Run.PounceLeaps;
        public int PounceBlows => Run.PounceBlows;
        public string DownedStruck => Run.DownedStruck;
        public int DownedBlows => Run.DownedBlows;
        public string GoMetFirst => Run.GoMetFirst;
        public bool StrikeDownedOnce { get => Run.StrikeDownedOnce; set => Run.StrikeDownedOnce = value; }

        /// <summary>What the mover leaves open to the other, as it is now (looked at ten times a second while the lab runs).</summary>
        public readonly System.Collections.Generic.List<PhysicsBattler.Locomotion.Openings.Seen> Open = new System.Collections.Generic.List<PhysicsBattler.Locomotion.Openings.Seen>();

        /// <summary>What the last posture watched opened and covered, and when (for the panel and the reports).</summary>
        public string Opened { get; private set; } = "none watched yet";

        /// <summary>How long a posture's openings are watched (seconds).</summary>
        public const float WatchesFor = 1.4f;

        private float openLooked = 1f, openWatch = -1f;
        private string openWatched = "";
        private readonly System.Collections.Generic.Dictionary<PhysicsBattler.Locomotion.AimSpot, bool> openWas = new System.Collections.Generic.Dictionary<PhysicsBattler.Locomotion.AimSpot, bool>();
        private readonly System.Collections.Generic.Dictionary<PhysicsBattler.Locomotion.AimSpot, (float from, float to)> openChanged = new System.Collections.Generic.Dictionary<PhysicsBattler.Locomotion.AimSpot, (float, float)>();

        /// <summary>The posture just begun is watched: which spots it opens that were covered, which it covers that were open, and when.</summary>
        public void WatchOpenings(string posture)
        {
            if (!Running || Mover.Clip == null || Other.Clip == null)
            {
                return;
            }

            LookAtOpenings();
            openWas.Clear();
            openChanged.Clear();
            foreach (var seen in Open)
            {
                openWas[seen.Spot] = seen.Open;
            }

            openWatched = posture;
            openWatch = 0f;
        }

        private void LookAtOpenings()
        {
            PhysicsBattler.Locomotion.Openings.Look(Mover.Clip, Other.Clip.PartAt(PhysicsBattler.Rig.PartId.UpperArmR), Open);
        }

        private void StepOpenings(float dt)
        {
            if (Mover.Clip == null || Other.Clip == null)
            {
                return;
            }

            openLooked += dt;
            if (openLooked < (openWatch >= 0f ? 0.033f : 0.1f))
            {
                return;
            }

            openLooked = 0f;
            LookAtOpenings();
            if (openWatch < 0f)
            {
                return;
            }

            openWatch += openWatch >= 0f ? Mathf.Max(dt, 0.033f) : 0f;
            foreach (var seen in Open)
            {
                if (openWas.TryGetValue(seen.Spot, out bool was) && seen.Open != was)
                {
                    openChanged[seen.Spot] = openChanged.TryGetValue(seen.Spot, out var span) ? (span.from, openWatch) : (openWatch, openWatch);
                }
            }

            if (openWatch >= WatchesFor)
            {
                var opened = new System.Text.StringBuilder();
                var covered = new System.Text.StringBuilder();
                foreach (var pair in openChanged)
                {
                    var into = openWas[pair.Key] ? covered : opened;
                    into.Append(into.Length > 0 ? ", " : "").Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "the {0} {1:0.00} to {2:0.00} s", PhysicsBattler.Locomotion.AimSpots.Name(pair.Key), pair.Value.from, pair.Value.to));
                }

                Opened = openWatched + ": opened " + (opened.Length > 0 ? opened.ToString() : "nothing") + "; covered " + (covered.Length > 0 ? covered.ToString() : "nothing");
                openWatch = -1f;
            }
        }

        /// <summary>
        /// What the lab's two do as they turn round quickly (<see cref="PhysicsBattler.Units.UnitController.TurnCuts"/>):
        /// 1, they only turn (as the lab begins: a turn is looked at as a turn); 2, every turn is made with a cut;
        /// 0, as in a fight. Until 17 November the lab's "just turns" left the fight's own rule on, and one turn in
        /// four came with a cut.
        /// </summary>
        public int TurnCuts = 1;

        /// <summary>
        /// To see a turn round on demand: the other is put, at once, this far round him from where he faces (degrees, +
        /// to his right), as far off as he was and facing him: an enemy at his side or behind him. His turn to face
        /// him is timed (<see cref="TurnBackText"/>). Standing or stepping. False: the lab is not set out.
        /// </summary>
        public bool PutOther(float degrees)
        {
            if (!Running || Mover == null || Other == null || Mover.Clip == null || Other.Clip == null)
            {
                return false;
            }

            // (Set out by hand: he stays where he is.)
            takesDistance = false;
            Vector3 to = Flat(Other.Root.position - Mover.Root.position);
            float way = Mover.Clip.Loco.Facing + degrees;
            Vector3 spot = Mover.Clip.Loco.Position + Quaternion.Euler(0f, way, 0f) * Vector3.forward * Mathf.Max(0.8f, to.magnitude);
            Other.Teleport(spot, way + 180f);
            // (A target stands on its spot and faces what it was set to face: both go with it.)
            Other.DummySpot = new Vector3(spot.x, Other.Root.position.y, spot.z);
            Other.Clip.Home = new Vector3(spot.x, Other.Clip.Home.y, spot.z);
            if (Other.DummyFace.HasValue)
            {
                Other.DummyFace = Mover.Root.position;
            }

            turnBackTimed = true;
            turnBackFrom = Mathf.Abs(Mathf.DeltaAngle(0f, degrees));
            turnBackFor = 0f;
            turnBackQuick = false;
            turnBackQuicks = Mover.Clip.Loco.QuickTurnsMade;
            return true;
        }

        /// <summary>
        /// The other is put, at once, straight ahead of him at this distance (metres at a Medium's size, root to root),
        /// facing him: the two set out for a bash on demand. False: the lab is not set out.
        /// </summary>
        public bool PutOtherAt(float metres)
        {
            if (!Running || Mover == null || Other == null || Mover.Clip == null || Other.Clip == null)
            {
                return false;
            }

            // (Not a body that is down or getting up: put at once, it was thrown back onto its pose where it lay.)
            if (Other.Clip.Fallen)
            {
                return false;
            }

            // (Set out by hand: he stays where he is.)
            takesDistance = false;
            float way = Mover.Clip.Loco.Facing;
            Vector3 spot = Mover.Clip.Loco.Position + Quaternion.Euler(0f, way, 0f) * Vector3.forward * (Mathf.Max(0.6f, metres) * Mover.Clip.Size);
            Other.Teleport(spot, way + 180f);
            Other.DummySpot = new Vector3(spot.x, Other.Root.position.y, spot.z);
            Other.Clip.Home = new Vector3(spot.x, Other.Clip.Home.y, spot.z);
            if (Other.DummyFace.HasValue)
            {
                Other.DummyFace = Mover.Root.position;
            }

            return true;
        }

        /// <summary>
        /// The same the other way about: he is put, at once, this far round the other from where the other faces
        /// (degrees, + to the other's right), as far off as he was and facing him. The other's turn to face him is
        /// the one to watch (whatever the other holds: every loadout turns the same way). False: the lab is not set out.
        /// </summary>
        public bool PutMover(float degrees)
        {
            if (!Running || Mover == null || Other == null || Mover.Clip == null || Other.Clip == null)
            {
                return false;
            }

            Vector3 to = Flat(Other.Root.position - Mover.Root.position);
            float way = Other.Clip.Loco.Facing + degrees;
            Vector3 spot = Other.Clip.Loco.Position + Quaternion.Euler(0f, way, 0f) * Vector3.forward * Mathf.Max(0.8f, to.magnitude);
            Mover.Teleport(spot, way + 180f);
            return true;
        }

        /// <summary>The last turn back in words: how far off the other was and how long he took to face him again (within 15 degrees).</summary>
        public string TurnBackText => turnBackTimed ? $"the other {turnBackFrom:0} degrees round, {turnBackFor:0.00} s so far"
            : turnBackFrom > 0f ? $"the other {turnBackFrom:0} degrees round, he faced him again in {turnBackFor:0.00} s ({(turnBackQuick ? "a quick turn" : "no quick turn")})" : "none yet";

        private bool turnBackTimed, turnBackQuick;
        private float turnBackFrom, turnBackFor;
        private int turnBackQuicks;

        private void StepTurnBack(float dt, Vector3 to)
        {
            if (!turnBackTimed || Mover.Clip == null || to.sqrMagnitude < 1e-4f)
            {
                return;
            }

            var loco = Mover.Clip.Loco;
            float off = Mathf.Abs(Mathf.DeltaAngle(loco.Facing, Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg));
            turnBackFor += dt;
            turnBackQuick = loco.QuickTurnsMade > turnBackQuicks;
            if (off < 15f || turnBackFor > 6f)
            {
                turnBackTimed = false;
            }
        }

        public void Step(float dt)
        {
            if (Running)
            {
                StepOpenings(dt);
            }

            if (!Running)
            {
                return;
            }

            StepCourse(dt);
            GiveStyle();
            if (Mover != null)
            {
                Mover.TurnCuts = TurnCuts;
            }

            if (Other != null)
            {
                Other.TurnCuts = TurnCuts;
            }

            Vector3 to = Flat(Other.Root.position - Mover.Root.position);
            Gap = to.magnitude;
            StepTurnBack(dt, to);
            Run.Mover = Mover;
            Run.Other = Other;
            // (Where a pounce can come, a stumbling man cannot guard: ClipFighter.StumblingOpensGuard.)
            if (Mover.Clip != null && Other.Clip != null)
            {
                Mover.Clip.StumbleOpensGuardHere = Other.Clip.StumbleOpensGuardHere = true;
                Mover.Clip.BladePassesHere = Other.Clip.BladePassesHere = true;
            }

            StepTwo(dt);
            Run.StepPounce(dt, RunIn.Pounces);
            Run.StepDowned(dt, RunIn.StrikesTheDowned);
            Vector3 moved = Flat(Mover.Root.position - last);
            last = Mover.Root.position;
            Speed = Mathf.Lerp(Speed, moved.magnitude / Mathf.Max(1e-4f, dt), Mathf.Clamp01(dt / 0.15f));
            Gone += Now == Order.Stand ? 0f : moved.magnitude;
            Fastest = Mathf.Max(Fastest, Speed);
            WatchStart(dt);
            if (Now != Order.Stand && since < 0.35f && dt > 1e-5f)
            {
                since += dt;
                Vector3 l = Mover.PartPosition(PhysicsBattler.Rig.PartId.FootL), r = Mover.PartPosition(PhysicsBattler.Rig.PartId.FootR), h = Mover.PartPosition(PhysicsBattler.Rig.PartId.Pelvis);
                SetOffFoot = Mathf.Max(SetOffFoot, Mathf.Max((l - lastFootL).magnitude, (r - lastFootR).magnitude) / dt);
                SetOffHips = Mathf.Max(SetOffHips, (h - lastHips).magnitude / dt);
                SetOffWent = Gone;
                if (Trace != null)
                {
                    var loco = Mover.Clip.Loco;
                    Trace.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00} s: left foot {1:0.0} m/s, right {2:0.0}, hips {3:0.0}; going {4:0.00} m/s; stride phase {5:0.00} at {6:0.00} a second, stride x{7:0.00}, {8}{9}",
                        since, (l - lastFootL).magnitude / dt, (r - lastFootR).magnitude / dt, (h - lastHips).magnitude / dt, Speed, loco.Phase, loco.PhaseRate, loco.StrideScale, loco.Current, loco.Running ? " running" : ""));
                }

                lastFootL = l;
                lastFootR = r;
                lastHips = h;
            }
            if (Mover.Clip != null)
            {
                // (An order to walk up, back away or go round is a walk kept up: walked, not stepped.)
                Mover.Clip.WalksOn = Now == Order.WalkTo || Now == Order.BackAway || Now == Order.CircleLeft || Now == Order.CircleRight;
            }

            // (Going for a man, his order waits: his legs are the run's.)
            bool hasHisLegs = Run.Step(dt);
            if (wasGoing && !Run.Going)
            {
                // (The going is over. With no order but to stand, he takes his distance again.)
                takesDistance = TakesDistanceAgain && Now == Order.Stand && !leaping && StoodOff > 0f;
                takenFrom = new Vector3(float.MaxValue, 0f, 0f);
                TookDistance = takesDistance ? "" : Now != Order.Stand ? "he went on with his order" : "he stood where it ended (old)";
            }

            wasGoing = Run.Going;
            if (hasHisLegs)
            {
                return;
            }

            StepDistance();

            if (leaping)
            {
                // (Until the leap's ground is covered; then he stands.)
                if (Mover.Clip == null || (!Mover.Clip.GivingGround && Speed < 0.15f && Mover.Clip.Loco.Current != PhysicsBattler.Locomotion.Locomotor.Mode.OneShot))
                {
                    // (Where the leap took him is his spot: he does not creep on to where it was meant to.)
                    leaping = false;
                    Stand();
                }

                return;
            }

            if (Now == Order.Stand || Gap < 1e-3f)
            {
                return;
            }

            Vector3 toward = to / Gap, left = Vector3.Cross(toward, Vector3.up);
            float pace = PaceNow;
            // (While he strikes his order does not fight the blow: it does not end under it, and going round he is
            // not steered back out to his distance while the blow steps him in. After it, he is.)
            bool striking = Mover.Clip != null && Mover.Clip.Striking;
            // (How soon he starts to slow for where he is to stop: sooner the faster he comes, as far as his walk coasts.)
            float slows = Mathf.Max(0.5f, 2f / Mathf.Max(1f, pace));
            switch (Now)
            {
                case Order.WalkTo:
                    if (Gap <= Near && !striking)
                    {
                        Stand();
                        return;
                    }

                    // (Slowing over the last of it, as the fight's walk in does.)
                    Mover.DummyGo = toward * Mathf.Min(pace, slows * 2f * (Gap - Near) + 0.2f);
                    break;
                case Order.BackAway:
                    if (Gap >= Far && !striking)
                    {
                        Stand();
                        return;
                    }

                    Mover.DummyGo = -toward * Mathf.Min(pace, slows * 2f * (Far - Gap) + 0.2f);
                    break;
                default:
                    // (Round the other at the distance it had. Going straight along the circle's edge it would leave the
                    // circle, the faster the further: it is steered inward by as much as the circle bends over the time
                    // its walk takes to answer, and in or out as it drifts.)
                    Vector3 round = (Now == Order.CircleLeft ? left : -left) * pace
                        + toward * Mathf.Clamp(pace * pace / Mathf.Max(0.5f, Mathf.Min(radius, striking ? Gap : radius)) * 0.45f + (striking ? 0f : 2.5f * (Gap - radius)), -0.6f * pace, pace);
                    Mover.DummyGo = round.normalized * pace;
                    break;
            }
        }
    }

    /// <summary>
    /// One fighter running at another and striking him in one motion (20 November): the pounce on an opening, and the
    /// blow at a man who is down. It was the duel lab's own; the dance's two fighters each have one now. Whoever owns
    /// it sets <see cref="Mover"/> and <see cref="Other"/>, calls <see cref="StepPounce"/> and <see cref="StepDowned"/>
    /// each step, and <see cref="Step"/>, which is true while it has the mover's legs (his own orders wait).
    /// </summary>
    public sealed class RunIn
    {
        /// <summary>The one who runs in, and the one he goes for.</summary>
        public UnitController Mover, Other;

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        // ---- Going for him in one motion (20 November, after Renier's look) ----

        /// <summary>
        /// Going for a man and striking him is one motion (Combat Feel "He Goes For Him At"; <c>FIGHT_GOESAT=3.6</c>).
        /// Renier, 20 November, of the first build, which leapt in, landed, and then struck: "he ran, stopped then
        /// attacked. That won't work. Similar to how the turn around and attack needed to be one motion so will this."
        /// Now he runs at the spot, and while he is still running throws the blow so that it lands as he comes into
        /// reach: thrown on the move, over his run (<see cref="PhysicsBattler.Locomotion.AttackOnTheMove"/>). He keeps
        /// coming through the blow and stops at arm's length. From any distance: a man knocked far is run after.
        /// Both the pounce and the blow at a man who is down go this way.
        /// </summary>
        public static float GoesForHimAt = DefaultGoesForHimAt;

        /// <summary>How fast he goes for him (m/s at a Medium's size), how far from the spot the blow is meant to land from, and how long the blow is taken to need from thrown to landed (metres at a Medium's size; seconds): he throws it that long before he is in reach.</summary>
        public const float DefaultGoesForHimAt = 3.6f, GoStrikesFrom = 1.1f, GoBlowTakes = 0.5f;

        /// <summary>
        /// Going to a place he must be at as the blow lands (beside a man who lies), he throws it this long before he
        /// would be there (seconds): the quickest blow he has lands about then, and for a slower one he slows as he
        /// comes, so that he is on the place either way. Thrown half a second out, a quick blow landed with him a
        /// third of a metre short of it.
        /// </summary>
        public const float GoBlowTakesToAPlace = 0.35f;

        /// <summary>The blow he throws on the run is one that lands within this long, if he has one (seconds).</summary>
        public const float GoBlowLandsWithin = 0.75f;

        /// <summary>
        /// A blow thrown on a run goes for a spot that is open (Combat Feel "A Blow On The Run Goes For What Is
        /// Open"; <c>FIGHT_GOOPEN=0</c> for the old way: always his chest). The pounce's blow was aimed at the chest of
        /// a man whose board was before his chest, and was blocked three times in four. Now, as he throws it, the
        /// spots it may be for are looked at in turn from his sword shoulder (<see cref="PhysicsBattler.Locomotion.Openings"/>)
        /// and the first one with nothing before it is taken.
        /// </summary>
        public static bool StrikesWhereOpen = true;

        /// <summary>The spots a pounce's blow may be for, the likeliest first; and those of the blow at a man who lies.</summary>
        private static readonly PhysicsBattler.Locomotion.AimSpot[] PounceSpots =
        {
            PhysicsBattler.Locomotion.AimSpot.Chest, PhysicsBattler.Locomotion.AimSpot.Head, PhysicsBattler.Locomotion.AimSpot.Neck, PhysicsBattler.Locomotion.AimSpot.ShoulderR,
            PhysicsBattler.Locomotion.AimSpot.UpperArmR, PhysicsBattler.Locomotion.AimSpot.Belly, PhysicsBattler.Locomotion.AimSpot.ThighR, PhysicsBattler.Locomotion.AimSpot.ThighL,
        };

        private static readonly PhysicsBattler.Locomotion.AimSpot[] DownedSpots =
        {
            PhysicsBattler.Locomotion.AimSpot.Chest, PhysicsBattler.Locomotion.AimSpot.Belly, PhysicsBattler.Locomotion.AimSpot.Hips, PhysicsBattler.Locomotion.AimSpot.Head, PhysicsBattler.Locomotion.AimSpot.Neck,
        };

        /// <summary>For the reports: each step of a blow thrown on the run, until it lands.</summary>
        public static System.Action<string> GoTrace;

        private bool going, goThrown, goLanded, goUpToSpeed;
        private PhysicsBattler.Locomotion.AimSpot goSpot;
        private PhysicsBattler.Locomotion.AttackClass goKind;
        private float goFor, goUntil, goRan, goFastest, goThrownAt, goSlowest, goGapWas, goLandedGoing, goLandedFrom;
        private string goWhy = "", goSaid = "", goMetBefore = "";
        private System.Action<string, bool> goTold;
        private System.Func<bool> goMayThrow, goOff;
        private System.Func<Vector3?> goStandAt;
        private string goBlow;
        private float goLunges, goLungesFrom, goMeasured;
        private PhysicsBattler.Locomotion.AimSpot[] goSpots;
        private bool goSoonest;
        private string goSpotSaid = "";
        private Vector3 goWasAt;
        private PhysicsBattler.Locomotion.AttackStyle? goStyle;

        /// <summary>He is going for the other now (the lab's orders wait).</summary>
        public bool Going => going;

        /// <summary>For the panel: what he is doing as he goes for him.</summary>
        public string GoingNow => !going ? "" : goThrown ? (goLanded ? "the blow has landed" : "the blow is thrown, he keeps coming") : "running at him";

        /// <param name="mayThrow">The blow waits for this (null: thrown as soon as he is near enough): until then he keeps coming, and stands at his striking distance.</param>
        /// <param name="off">It is called off when this turns true before the blow is thrown (null: never).</param>
        private void GoFor(string why, PhysicsBattler.Locomotion.AimSpot spot, PhysicsBattler.Locomotion.AttackClass kind, float atMost, float gap, System.Action<string, bool> told, System.Func<bool> mayThrow = null, System.Func<bool> off = null,
            System.Func<Vector3?> standAt = null, string blow = null, PhysicsBattler.Locomotion.AttackStyle? style = null, float lunges = 0f, float lungesFrom = 0f)
        {
            goLunges = lunges;
            goLungesFrom = lungesFrom;
            goSpots = null;
            goSoonest = false;
            goSpotSaid = "";
            goMeasured = -1f;
            goStandAt = standAt;
            goBlow = blow;
            goStyle = style;
            going = true;
            goThrown = goLanded = goUpToSpeed = false;
            goFor = goRan = goFastest = 0f;
            goSlowest = float.MaxValue;
            goUntil = atMost;
            goWhy = why;
            goSpot = spot;
            goKind = kind;
            goGapWas = gap;
            goTold = told;
            goMayThrow = mayThrow;
            goOff = off;
        }

        private void EndGoing(string said, bool struck)
        {
            going = false;
            var told = goTold;
            goTold = null;
            goMayThrow = goOff = null;
            goStandAt = null;
            if (Mover != null)
            {
                Mover.DummyGo = null;
                Mover.DummySpot = Mover.Root.position;
            }

            told?.Invoke(said, struck);
        }

        /// <summary>A step of going for him. True while it has the mover's legs (his order waits).</summary>
        public bool Step(float dt)
        {
            if (!going)
            {
                return false;
            }

            var mine = Mover != null ? Mover.Clip : null;
            var his = Other != null ? Other.Clip : null;
            goFor += dt;
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            if (mine == null || his == null || mine.Fallen || mine.Stumbling)
            {
                EndGoing(goWhy + ": he could not go on (down or stumbling himself)", false);
                return false;
            }

            Vector3 spot = PhysicsBattler.Locomotion.AimSpots.Point(his, goSpot);
            Vector3 to = Flat(spot - Mover.Root.position);
            float gap = to.magnitude;
            Vector3 way = gap > 1e-3f ? to / gap : Vector3.forward;
            float size = mine.Size, from = GoStrikesFrom * size, pace = GoesForHimAt * Mathf.Sqrt(Mathf.Max(0.1f, size));
            float speed = Flat(mine.Loco.Velocity).magnitude;
            // (How fast he is really going, over the last tenth of a second: a blow with its own lunge carries him by
            // its recording, which his walk's speed does not show.)
            float moved = dt > 1e-5f ? Flat(Mover.Root.position - goWasAt).magnitude / dt : speed;
            goMeasured = goMeasured < 0f ? speed : Mathf.Lerp(goMeasured, moved, Mathf.Clamp01(dt / 0.1f));
            goWasAt = Mover.Root.position;
            // (Where he means to be as the blow lands: his striking distance short of the spot on his own line to it,
            // or the place he was given: beside the chest of a man who lies.)
            Vector3? given = goStandAt != null ? goStandAt() : null;
            Vector3 toStand = given.HasValue ? Flat(given.Value - Mover.Root.position) : way * (gap - from);
            float off = toStand.magnitude;
            Vector3 goes = off > 1e-3f ? toStand / off : way;
            mine.WalksOn = true;
            // (His own bash or kick has done its work: he does not stand through the rest of it.)
            mine.CutBashShort();
            mine.CutKickShort();
            if (!goThrown)
            {
                if (goOff != null && goOff())
                {
                    EndGoing(string.Format(inv, "{0}, {1:0.00} m off: he went {2:0.00} m toward him, and it was over before he was near enough", goWhy, goGapWas, goRan), false);
                    return false;
                }

                if (goFor > goUntil)
                {
                    EndGoing(string.Format(inv, "{0}, {1:0.00} m off: he went {2:0.00} m toward him and was not in reach in {3:0.0} s", goWhy, goGapWas, goRan, goUntil), false);
                    return false;
                }

                // (He runs at him, and does not run onto him: with the blow still to wait for, he comes to his striking distance and no nearer.)
                Mover.DummyGo = goes * Mathf.Min(4f * off, pace);
                goRan += speed * dt;
                goFastest = Mathf.Max(goFastest, speed);
                if (speed >= 0.6f * pace)
                {
                    goUpToSpeed = true;
                    goSlowest = float.MaxValue;
                }
                else if (goUpToSpeed)
                {
                    goSlowest = Mathf.Min(goSlowest, speed);
                }

                bool free = !mine.Striking && !mine.Bashing && !mine.Kicking && mine.Loco.Ready && mine.Loco.Current != PhysicsBattler.Locomotion.Locomotor.Mode.OneShot;
                // (Thrown while he is still coming, as long before he is in reach as the blow takes.)
                // (Or, with a blow that has its own lunge: thrown from that far off the spot, the lunge carrying him in.)
                if (free && (goLunges > 0f ? gap <= goLungesFrom * size : off <= speed * (given.HasValue ? GoBlowTakesToAPlace : GoBlowTakes) + 0.08f * size) && (goMayThrow == null || goMayThrow()))
                {
                    goMetBefore = mine.Strikes != null ? mine.Strikes.Last : "";
                    WatchWhatItMeets(mine);
                    // (Struck where he is open, of the spots it may be for: the first of them that a straight line
                    // from his sword shoulder reaches with nothing of the other before it.)
                    if (StrikesWhereOpen && goSpots != null)
                    {
                        Vector3 shoulder = mine.Anim.Part(PhysicsBattler.Rig.PartId.UpperArmR).position;
                        goSpotSaid = "; nothing of him was open, so";
                        foreach (var tried in goSpots)
                        {
                            if (PhysicsBattler.Locomotion.Openings.LookAt(his, shoulder, tried).Open)
                            {
                                goSpotSaid = tried == goSpots[0] ? "" : "; his " + PhysicsBattler.Locomotion.AimSpots.Name(goSpots[0]) + " was covered, so";
                                goSpot = tried;
                                spot = PhysicsBattler.Locomotion.AimSpots.Point(his, goSpot);
                                break;
                            }
                        }
                    }

                    // (A blow that lands as he arrives: not one whose wind-up he would have to stand and wait through.)
                    PhysicsBattler.Locomotion.AttackOnTheMove.LandsWithin = GoBlowLandsWithin;
                    PhysicsBattler.Locomotion.AttackOnTheMove.TakesSoonest = goSoonest;
                    bool thrown = Mover.AimStrike(Other, goSpot, out string strike, out float miss, attack: goBlow, landIn: goLunges, kind: goKind, style: goStyle);
                    PhysicsBattler.Locomotion.AttackOnTheMove.LandsWithin = 0f;
                    PhysicsBattler.Locomotion.AttackOnTheMove.TakesSoonest = false;
                    if (thrown)
                    {
                        goThrown = true;
                        goThrownAt = goFor;
                        goSlowest = Mathf.Min(goSlowest, speed);
                        goSaid = string.Format(inv, "{0}, {1:0.00} m off: he {2}, and threw the blow {3:0.00} s after he set off" + goSpotSaid + " at his " + PhysicsBattler.Locomotion.AimSpots.Name(goSpot) + ", {4:0.00} m from the spot ({5} cm over the ground), going {6:0.0} m/s ({7}{8}; {9})",
                            goWhy, goGapWas, goRan > 0.3f ? string.Format(inv, "ran {0:0.00} m at up to {1:0.0} m/s", goRan, goFastest) : "was in reach",
                            goFor, gap, Mathf.RoundToInt(100f * (spot.y - Mover.Root.position.y)), speed, strike,
                            mine.MovedPlanted ? ", planted for it" : mine.Loco.ShotOverWalk ? ", thrown over his run" : goLunges > 0f && speed > 0.8f ? ", his run carried into its lunge" : ", thrown from his stand",
                            miss > 0.005f ? string.Format(inv, "expected to fall {0:0} cm short", 100f * miss) : "expected to reach");
                    }
                }

                return true;
            }

            // (Through the blow he keeps coming; once it has landed he stops.)
            var made = mine.Made;
            bool landed = made != null && mine.Loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.OneShot && mine.Loco.Shot == made.Clip && mine.Loco.ShotTime >= made.ContactAt;
            if (!goLanded && (landed || !mine.Striking))
            {
                goLanded = true;
                goLandedGoing = goLunges > 0f ? goMeasured : speed;
                goLandedFrom = gap;
                goLandedWhere = WhereByHim(his, spot);
            }

            if (!goLanded)
            {
                goSlowest = Mathf.Min(goSlowest, goLunges > 0f ? goMeasured : speed);
                // (He comes on so as to be at his striking distance as it lands, wherever the other has got to by then.)
                float left = made != null && mine.Loco.Shot == made.Clip ? Mathf.Max(0.08f, (made.ContactAt - mine.Loco.ShotTime) / Mathf.Max(0.2f, Mathf.Abs(mine.Loco.ShotSpeed))) : 0.2f;
                Mover.DummyGo = goLunges > 0f ? Vector3.zero : goes * Mathf.Clamp(off / left, 0f, pace);
                GoTrace?.Invoke(string.Format(inv, "{0:0.00} s: {1:0.00} m from the spot, going {2:0.00} m/s (moved at " + moved.ToString("0.00", inv) + "), asked {3:0.00}; the blow {4:0.00} s in, lands at {5:0.00}, speed {6:0.00}, planned to land in {7:0.00}; legs {8}, over his walk {9}",
                    goFor - goThrownAt, gap, speed, Mover.DummyGo.Value.magnitude, mine.Loco.ShotTime, made != null ? made.ContactAt : -1f, mine.Loco.ShotSpeed, made != null ? made.Asked.LandIn : -1f, mine.Loco.Legs, mine.Loco.ShotOverWalk));
            }
            else
            {
                Mover.DummyGo = Vector3.zero;
            }

            if (!mine.Striking && goFor > goThrownAt + 0.25f)
            {
                string met = goMet.Count > 0 ? string.Join(", then ", goMet) : mine.Strikes != null && mine.Strikes.Last != goMetBefore ? mine.Strikes.Last : "";
                GoMetFirst = goMet.Count > 0 ? goMet[0] : "";
                EndGoing(goSaid + string.Format(inv, "; from up to speed until it landed he never went slower than {0:0.0} m/s; it landed with him going {1:0.0} m/s, {2:0.00} m from the spot{4}; {3}",
                    goSlowest > 1e6f ? speed : goSlowest, goLandedGoing, goLandedFrom, met.Length > 0 ? "it met: " + (met.Length > 160 ? met.Substring(0, 160) : met) : "it met nothing", goLandedWhere), true);
                return false;
            }

            return true;
        }

        // ---- A man who is down (the Downed play plan, a first piece of its steps 0 and 1: 20 November) ----

        /// <summary>
        /// He goes for a man who is down (Combat Feel "A Fighter Goes For A Man Who Is Down"; on by default in the game
        /// since Renier's decision of 20 November; this static stays off until Combat Feel or a headless lever sets it,
        /// so that headless falls and get-ups are of a man left alone unless FIGHT_DOWNED=1 asks). On, or asked for
        /// once from the lab ("Strike him where he lies", or the one-press "Knock him down and strike him"): once the
        /// other lies off his feet, the mover runs to him from wherever he is and strikes down at his chest as he
        /// arrives, in one motion (<see cref="GoesForHimAt"/>), with the blows there are today aimed at the spot where
        /// it lies. Once for each time he is down.
        /// </summary>
        public static bool StrikesTheDowned = false;

        /// <summary>From how far off he goes for a man who is down (metres at a Medium's size): as far as a hard bash throws one, and more.</summary>
        public const float DownedNear = 20f;

        /// <summary>Asked from the lab: the next time the other is down, he is struck once, whatever the switch says.</summary>
        public bool StrikeDownedOnce;

        /// <summary>For the panel and the reports: what the last blow at a man who was down was, and how many were thrown.</summary>
        public string DownedStruck { get; private set; } = "none yet";
        public int DownedBlows { get; private set; }

        private bool downedDone;

        /// <summary>How long he must have been down, and how low his chest must lie, before he is gone for (seconds; metres at a Medium's size): gone for while he was still in the air, the blow fell a metre short.</summary>
        public const float DownedLiesFor = 0.45f, DownedLiesUnder = 0.5f;
        private float downedFor = -1f;

        /// <param name="strikes">He strikes a man who is down (the lab's or the dance's own switch).</param>
        public void StepDowned(float dt, bool strikes)
        {
            var mine = Mover != null ? Mover.Clip : null;
            var his = Other != null ? Other.Clip : null;
            if (mine == null || his == null || !his.Fallen)
            {
                downedDone = false;
                downedFor = -1f;
                return;
            }

            downedFor = downedFor < 0f ? 0f : downedFor + dt;
            if (downedDone || going || !(strikes || StrikeDownedOnce) || mine.Fallen || mine.Stumbling)
            {
                return;
            }

            Vector3 spot = PhysicsBattler.Locomotion.AimSpots.Point(his, PhysicsBattler.Locomotion.AimSpot.Chest);
            float gap = Flat(spot - Mover.Root.position).magnitude;
            if (gap > DownedNear * mine.Size)
            {
                return;
            }

            // (He sets off at once, while the other is still falling: the blow waits until he lies.)
            downedDone = true;
            besideSide = 0f;
            StrikeDownedOnce = false;
            GoFor("the other down", PhysicsBattler.Locomotion.AimSpot.Chest, PhysicsBattler.Locomotion.AttackClass.Plain,
                gap / Mathf.Max(0.5f, GoesForHimAt) + 4f, gap, (said, struck) =>
                {
                    DownedStruck = said;
                    if (struck)
                    {
                        DownedBlows++;
                    }

                    if (PhysicsBattler.Diagnostics.PlaytestTrace.Enabled)
                    {
                        PhysicsBattler.Diagnostics.PlaytestTrace.Note(Mover, "downed", said);
                    }
                },
                () => Other != null && Other.Clip != null && downedFor >= DownedLiesFor
                    && PhysicsBattler.Locomotion.AimSpots.Point(Other.Clip, PhysicsBattler.Locomotion.AimSpot.Chest).y - Mover.Root.position.y <= DownedLiesUnder * Other.Clip.Size,
                () => Other == null || Other.Clip == null || !Other.Clip.Fallen,
                DownedComesBeside ? BesideHisChest : (System.Func<Vector3?>)null,
                !DownedComesBeside || string.IsNullOrEmpty(DownedBlow) ? null : DownedBlow,
                DownedComesBeside && (DownedCrouch > 0.001f || Mathf.Abs(DownedLean - 1f) > 0.001f) ? DownedStyle() : (PhysicsBattler.Locomotion.AttackStyle?)null,
                DownedComesBeside ? DownedLunges : 0f, DownedLungesFrom);
            goSpots = DownedComesBeside ? DownedSpots : null;
        }

        /// <summary>The blow at a man who is down has its own lunge, his run carried into it: how long from thrown to landed (seconds; 0: thrown over his run, as the pounce's is), and how far from the spot it is thrown (metres at a Medium's size).</summary>
        public static float DownedLunges = DefaultDownedLunges, DownedLungesFrom = DefaultDownedLungesFrom;
        public const float DefaultDownedLunges = 0f, DefaultDownedLungesFrom = 1.7f;

        /// <summary>
        /// The blow at a man who is down is a blow of its own (the Downed play plan's step 1; 20 November; Combat Feel
        /// "A Man Who Is Down Is Struck From Beside Him"; <c>FIGHT_DOWNEDBLOW</c>). The first two builds ran straight
        /// at him and threw a standing blow aimed low: a man knocked back lies with his feet toward the one who
        /// knocked him, so the blade met a shin or an arm before his chest. Now he runs to a place beside the chest,
        /// square to how the man lies and on the nearer side, and strikes down from there, bent to it: crouched and
        /// leaning in (<see cref="DownedCrouch"/>, <see cref="DownedLean"/>), with the blow named in
        /// <see cref="DownedBlow"/> (none: the best fit of his plain blows): his thrust, the point going down: a stab.
        /// It goes for the first of his chest, belly, hips, head and neck that is open from there. Off: straight at
        /// him with a standing blow aimed low, as the first two builds had it.
        /// <para>Measured headless over six knock-downs (a man with a sword, with a shield, bare-handed; kicked
        /// 3.5 m and 5 m off): what the blow met first was his trunk or head in two of six the old way (the others an
        /// arm, a hand, nothing) and in five of six this way (three his chest, two his head, one a thigh).</para>
        /// </summary>
        public static bool DownedComesBeside = true;

        /// <summary>Which recorded blow is thrown at a man who is down ("" or null: the best fit of the plain ones), how much lower his hips are carried through it (leg lengths), and how far his chest leans in (times the recording's).</summary>
        public static string DownedBlow = DefaultDownedBlow;
        public static float DownedCrouch = DefaultDownedCrouch, DownedLean = DefaultDownedLean;

        public const string DefaultDownedBlow = "sns_thrust";
        public const float DefaultDownedCrouch = 0.2f, DefaultDownedLean = 1.8f;

        /// <summary>How far from the chest of a man who lies he stands to strike it, square to how the man lies (metres at a Medium's size).</summary>
        public static float DownedStandsOff = DefaultDownedStandsOff;
        public const float DefaultDownedStandsOff = 0.8f;

        /// <summary>How far toward the lying man's head from his chest the place is (metres at a Medium's size; under 0: toward his feet).</summary>
        public static float DownedTowardHead = DefaultDownedTowardHead;
        public const float DefaultDownedTowardHead = 0f;

        private static PhysicsBattler.Locomotion.AttackStyle DownedStyle()
        {
            var style = PhysicsBattler.Locomotion.AttackClasses.StyleOf(PhysicsBattler.Locomotion.AttackClass.Plain);
            style.Crouch += DownedCrouch;
            style.Lean *= DownedLean;
            return style;
        }

        /// <summary>The place beside the chest of the man who lies: square to his length, on the side the mover is nearer to. Null: he does not lie along any line (taken straight).</summary>
        private Vector3? BesideHisChest()
        {
            var his = Other != null ? Other.Clip : null;
            if (his == null || Mover == null || Mover.Clip == null)
            {
                return null;
            }

            Vector3 chest = PhysicsBattler.Locomotion.AimSpots.Point(his, PhysicsBattler.Locomotion.AimSpot.Chest);
            Vector3 along = Flat(his.Rig.Part(PhysicsBattler.Rig.PartId.Head).transform.position - his.Rig.Part(PhysicsBattler.Rig.PartId.Pelvis).transform.position);
            if (along.sqrMagnitude < 0.01f)
            {
                return null;
            }

            Vector3 side = Vector3.Cross(Vector3.up, along.normalized);
            Vector3 me = Flat(Mover.Root.position - chest);
            // (The side he is on already; right over the man's line, the side his sword arm is on as he comes.)
            float mine = Vector3.Dot(me, side);
            if (Mathf.Abs(mine) < 0.15f)
            {
                mine = besideSide != 0f ? besideSide : 1f;
            }

            besideSide = Mathf.Sign(mine);
            Vector3 at = chest + side * (besideSide * DownedStandsOff * Mover.Clip.Size) + along.normalized * (DownedTowardHead * Mover.Clip.Size);
            at.y = Mover.Root.position.y;
            return at;
        }

        private float besideSide;
        private string goLandedWhere = "";
        private readonly System.Collections.Generic.List<string> goMet = new System.Collections.Generic.List<string>();
        private PhysicsBattler.Locomotion.StrikeHits goWatched;

        /// <summary>For the reports: the first thing the last blow thrown on a run met ("": nothing).</summary>
        public string GoMetFirst { get; private set; } = "";

        /// <summary>Everything the blow about to be thrown meets is written down, in the order it meets it: what a blow "met" was the last part it went through, not the first.</summary>
        private void WatchWhatItMeets(PhysicsBattler.Locomotion.ClipFighter mine)
        {
            goMet.Clear();
            if (goWatched != null)
            {
                goWatched.Struck -= OnGoStruck;
            }

            goWatched = mine.Strikes;
            if (goWatched != null)
            {
                goWatched.Struck += OnGoStruck;
            }
        }

        private void OnGoStruck(PhysicsBattler.Locomotion.StrikeHits.StruckBy by)
        {
            if (goMet.Count < 6)
            {
                goMet.Add((by.Parried ? "his parry" : by.Shield ? "his shield" : "his " + by.Part.ToString()) + string.Format(System.Globalization.CultureInfo.InvariantCulture, " ({0:0} cm up, at {1:0.0} m/s{2})", 100f * (by.Point.y - (Mover != null ? Mover.Root.position.y : 0f)), by.Speed,
                    by.Struck != null && !by.Struck.Fallen ? (by.Struck.Stumbling ? "; he was stumbling" : "; he was on his feet again") + (by.Struck.GuardOpen ? ", his guard open" : ", his guard up") : ""));
            }
        }

        /// <summary>For the reports: where the mover is by a man who lies: how far to his side and how far toward his head or his feet from the spot, and how far the mover is turned from facing it. "" if the other stands.</summary>
        private string WhereByHim(PhysicsBattler.Locomotion.ClipFighter his, Vector3 spot)
        {
            if (his == null || !his.Fallen || Mover == null || Mover.Clip == null)
            {
                return "";
            }

            Vector3 along = Flat(his.Rig.Part(PhysicsBattler.Rig.PartId.Head).transform.position - his.Rig.Part(PhysicsBattler.Rig.PartId.Pelvis).transform.position);
            if (along.sqrMagnitude < 0.01f)
            {
                return "";
            }

            along.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, along);
            Vector3 me = Flat(Mover.Root.position - spot);
            float toHead = Vector3.Dot(me, along), toSide = Vector3.Dot(me, side);
            float faces = Mathf.Abs(Mathf.DeltaAngle(Mover.Clip.Loco.Facing, Mathf.Atan2(-me.x, -me.z) * Mathf.Rad2Deg));
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, " ({0:0.00} m to his {1} side and {2:0.00} m toward his {3}, turned {4:0} degrees from facing it)",
                Mathf.Abs(toSide), toSide > 0f ? "left" : "right", Mathf.Abs(toHead), toHead > 0f ? "head" : "feet", faces);
        }

        // ---- The pounce (the fight rhythm plan's step 10: a first build, 20 November) ----

        /// <summary>
        /// He pounces on an opening (Combat Feel "A Fighter Pounces On An Opening"; the duel lab only until the fight of
        /// six takes it; <c>FIGHT_POUNCE=0</c> for the old way). The other stumbling, his guard knocked open, or his
        /// back turned is an opening. The mover goes for it: he runs in and strikes as he arrives, in one motion
        /// (<see cref="GoesForHimAt"/>), a light blow at the chest; in reach he strikes from where he stands. Once for
        /// each opening. A man on the ground is not an opening here: that is Downed play. Before, he stood off at his
        /// guard's distance after a bash and let the other find his feet.
        /// </summary>
        public static bool Pounces = true;

        /// <summary>How soon he reads an opening (seconds: the lever "Pounce reads in"), from how far off he will go for it (metres at a Medium's size), how long he keeps coming once he has set off, and how far round the other must be turned from him for his back to count (degrees).</summary>
        public static float PounceReadsIn = DefaultPounceReadsIn;

        /// <summary>A stumble is an opening to pounce on once it has driven him this far (metres at a Medium's size; Combat Feel "A Stumble Is An Opening From"; 0: any stumble, as it was).</summary>
        public static float PounceFromStumble = 0.6f;

        private bool stumbleSeen;
        private Vector3 stumbleFrom;

        public const float DefaultPounceReadsIn = 0.15f, PounceFromAtMost = 6f, PounceCommitted = 1.6f, PounceBackTurned = 120f;

        /// <summary>For the panel and the reports: what the last pounce was, and how many openings were seen, gone for and struck at.</summary>
        public string Pounced { get; private set; } = "none yet";
        public int OpeningsSeen { get; private set; }
        public int PounceLeaps { get; private set; }
        public int PounceBlows { get; private set; }

        private float openFor = -1f;
        private bool pounceGone;

        /// <param name="pounces">He pounces on an opening (the lab's or the dance's own switch).</param>
        public void StepPounce(float dt, bool pounces)
        {
            var mine = Mover != null ? Mover.Clip : null;
            var his = Other != null ? Other.Clip : null;
            if (!pounces || mine == null || his == null)
            {
                openFor = -1f;
                return;
            }

            Vector3 to = Flat(Other.Root.position - Mover.Root.position);
            // (An opening: he is on his feet and stumbling, his guard is knocked open, or his back is to the mover.)
            float gap = to.magnitude;
            Vector3 way = gap > 1e-3f ? to / gap : Vector3.forward;
            float turned = Mathf.Abs(Mathf.DeltaAngle(his.Loco.Facing, Mathf.Atan2(-way.x, -way.z) * Mathf.Rad2Deg));
            // (A stumble is an opening once it has driven him far enough: Renier, 21 November, "the attack that is
            // triggered by the stumbles is a bit too sensitive. The stumble should be a bit more before the stumble
            // attack happens." His log: 33 pounces in one sitting, on nearly every blow that landed.)
            if (his.Stumbling && !stumbleSeen)
            {
                stumbleSeen = true;
                stumbleFrom = Other.Root.position;
            }
            else if (!his.Stumbling)
            {
                stumbleSeen = false;
            }

            float driven = stumbleSeen ? Flat(Other.Root.position - stumbleFrom).magnitude : 0f;
            bool stumbledEnough = his.Stumbling && driven >= PounceFromStumble * his.Size;
            string why = his.Fallen ? "" : stumbledEnough ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "he was stumbling ({0:0.00} m driven)", driven) : his.GuardOpen ? "his guard was knocked open" : turned >= PounceBackTurned ? "his back was turned" : "";
            if (why.Length == 0)
            {
                if (openFor >= 0f && !pounceGone)
                {
                    Pounced = $"an opening closed after {openFor:0.00} s before he was free to take it";
                }

                openFor = -1f;
                return;
            }

            if (openFor < 0f)
            {
                openFor = 0f;
                pounceGone = false;
                OpeningsSeen++;
            }

            openFor += dt;
            if (pounceGone || going || openFor < PounceReadsIn || gap > PounceFromAtMost * mine.Size || mine.Fallen || mine.Stumbling)
            {
                return;
            }

            pounceGone = true;
            PounceLeaps++;
            GoFor(why, PhysicsBattler.Locomotion.AimSpot.Chest, PhysicsBattler.Locomotion.AttackClass.Light, PounceCommitted, gap, (said, struck) =>
            {
                Pounced = said;
                if (struck)
                {
                    PounceBlows++;
                }

                if (PhysicsBattler.Diagnostics.PlaytestTrace.Enabled)
                {
                    PhysicsBattler.Diagnostics.PlaytestTrace.Note(Mover, "pounce", said);
                }
            }, null, () => Other == null || Other.Clip == null || Other.Clip.Fallen);
            goSpots = PounceSpots;
            // (An opening does not wait: of the blows he can throw on the run, the one that lands soonest.)
            goSoonest = true;
        }
    }
}
