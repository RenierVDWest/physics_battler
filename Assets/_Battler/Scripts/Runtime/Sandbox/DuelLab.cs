using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
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

            string now = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1} {2:0.00} {3:0.00}", Style.Name, PhysicsBattler.Locomotion.MoveStyle.GingaTake,
                PhysicsBattler.Locomotion.MoveStyle.StandSize, PhysicsBattler.Locomotion.MoveStyle.StandArms);
            if (Mover.Clip.Loco.Style != Style || now != styleGiven)
            {
                Mover.Clip.Loco.SetStyle(Style);
                styleGiven = now;
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

        public void Begin(UnitController mover, UnitController other)
        {
            Mover = mover;
            Other = other;
            Now = Order.Stand;
            Gone = 0f;
            if (mover != null)
            {
                last = mover.Root.position;
            }
        }

        public void Stop()
        {
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
            BlowThrown = loco.ShotOverWalk && made != null
                ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "thrown on the move ({0}, to land {1:0.00} s after, {2})", made.Clip.Name.Replace("made_sns_", ""), made.LandsIn,
                    made.StepIn.magnitude * loco.Leg < 0.03f ? "no step in" : string.Format(System.Globalization.CultureInfo.InvariantCulture, "a step {0} of {1:0} cm", Vector3.Dot(made.StepIn, made.Asked.Target) >= 0f ? "in" : "back", 100f * made.StepIn.magnitude * loco.Leg))
                : Mover.Clip.MovedShort > 0f
                    ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "planted (on the move it would have fallen {0:0} cm short)", 100f * Mover.Clip.MovedShort)
                    : PhysicsBattler.Locomotion.AttackOnTheMove.On && Speed < PhysicsBattler.Locomotion.AttackOnTheMove.WalksFrom ? "planted (he was not walking)" : "planted";
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

        private void Stand()
        {
            // (Where it is now is its spot: it does not walk back to where it began.)
            Now = Order.Stand;
            Mover.DummyGo = null;
            Mover.DummySpot = Mover.Root.position;
        }

        /// <summary>One step of the lab (the physics step): the order turned into the velocity the mover is steered by.</summary>
        public void Step(float dt)
        {
            if (!Running)
            {
                return;
            }

            GiveStyle();
            Vector3 to = Flat(Other.Root.position - Mover.Root.position);
            Gap = to.magnitude;
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
}
