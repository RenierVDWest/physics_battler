using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A fighter pushed off balance (the Fight Rhythm plan's step 9c, "stumbles and falls"; the impacts plan's "off
    /// balance"). His body is a weight on legs: it tips away from the feet that carry it, faster the further it is
    /// past them, and the only thing that stops it is a foot put down ahead of it. So he steps: the foot behind lifts
    /// and goes for the point where a foot would stop him (his body's place plus its speed times about a third of a
    /// second), as far as a leg reaches and as fast as a foot goes. A foot that lands ahead of him takes speed out of
    /// him by how far ahead it is (the leg is a strut: what of his speed runs along it is lost) and his knees give.
    /// If the foot gets there, he is caught, in one step or several. If his body outruns his feet, both are off the
    /// ground, his hips sink, and he is down.
    /// Nothing is drawn by lot. What he does comes from the speed and the twist the impact left him with and from
    /// where his feet were when it came.
    /// </summary>
    public sealed class StumbleSteps
    {
        // ---- The levers (the scene's Combat Feel) -----------------------------------------------------------------

        /// <summary>A fighter pushed off balance stumbles on real stumble steps (Combat Feel "Off Balance He Stumbles On Real Steps"). Off: the guard's walk played fast, as before.</summary>
        public static bool On = true;

        /// <summary>How long after he is pushed his first foot lifts (seconds).</summary>
        public static float ReactsIn = DefaultReactsIn;

        /// <summary>The shortest a stumble step is in the air (seconds).</summary>
        public static float StepTakes = DefaultStepTakes;

        /// <summary>How far from under his hips a foot can be put down (leg lengths, flat).</summary>
        public static float StepReach = DefaultStepReach;

        /// <summary>The fastest a foot goes over the ground in a stumble step (m/s at a Medium's size).</summary>
        public static float FootSpeed = DefaultFootSpeed;

        /// <summary>How much of what a foot landing ahead of him can take out of his speed it does take (1: a stiff leg, all of it; 0: none).</summary>
        public static float LegsBrake = DefaultLegsBrake;

        /// <summary>How far his knees give as a foot takes his weight (metres of hips for each m/s the leg takes, about).</summary>
        public static float KneesGive = DefaultKneesGive;

        /// <summary>How far his hips may sink before he is down (leg lengths).</summary>
        public static float DownAt = DefaultDownAt;

        /// <summary>How much of an impact's twist turns the whole of him round, the rest being taken up in his trunk (0 to 1).</summary>
        public static float TurnedBy = DefaultTurnedBy;

        public const float DefaultReactsIn = 0.1f, DefaultStepTakes = 0.18f, DefaultStepReach = 0.5f, DefaultFootSpeed = 5f, DefaultLegsBrake = 1f,
            DefaultKneesGive = 0.05f, DefaultDownAt = 0.42f, DefaultTurnedBy = 0.25f;

        /// <summary>
        /// A sole's reach about its ankle (metres at a Medium's size): how far the weight can be carried past a foot
        /// before it tips. How long both feet are down between two stumble steps (seconds). How far past the point
        /// that would stop him a foot is aimed (metres). How near his other foot a foot may be put down (metres).
        /// How far behind him a foot is left before it comes off the ground by itself (leg lengths).
        /// </summary>
        public const float Sole = 0.09f, Between = 0.03f, Past = 0.05f, Clear = 0.2f, LeavesAt = 0.74f;

        /// <summary>He is caught when his body is slower than this (m/s at a Medium's size) over feet that hold it.</summary>
        public const float CaughtBelow = 0.45f;

        /// <summary>How fast a twist dies with both feet on the ground, and with one (degrees a second, each second): the ground's grip on his soles.</summary>
        public const float GripBoth = 2400f, GripOne = 600f;

        /// <summary>The most he is turning as it begins (degrees a second).</summary>
        public const float MostSpin = 540f;

        public const float Gravity = 9.81f;

        /// <summary>How a stumble ended.</summary>
        public enum Ends
        {
            None,
            Caught,
            Down,
        }

        // ---- What it is doing -------------------------------------------------------------------------------------

        public bool Active { get; private set; }

        public Ends Ended { get; private set; }

        /// <summary>Why he went down, in words (for the log).</summary>
        public string Why { get; private set; } = "";

        /// <summary>His body over the ground and its speed (world, flat).</summary>
        public Vector3 Com, Vel;

        /// <summary>The way he faces (degrees) and how fast that is turning (degrees a second).</summary>
        public float Facing, Spin;

        /// <summary>Each ankle over the ground (world; flat, and y is how high it is lifted), whether that foot is down, and the way it points (degrees).</summary>
        public readonly Vector3[] Foot = new Vector3[2];

        public readonly bool[] Down = new bool[2];

        public readonly float[] FootYaw = new float[2];

        /// <summary>Steps taken, seconds it has gone on, how far it carried him (metres) and turned him (degrees), and the fastest he went (m/s).</summary>
        public int Steps;

        public float For, Went, Turned, Fastest;

        /// <summary>How far his hips are down for it now (metres): knees giving as a foot lands, and falling while no foot is under him.</summary>
        public float Sink { get; private set; }

        /// <summary>
        /// How far his weight is past what carries it, as a body tipping over its feet: the way it tips (world, flat)
        /// times the angle (radians). Past a foot behind him it tips the way he goes; short of a foot landed ahead of
        /// him it tips back against it.
        /// </summary>
        public Vector3 Tilt { get; private set; }

        /// <summary>How much of that tipping his upper body shows, his hips bending for the rest (0 to 1).</summary>
        public static float TrunkTips = DefaultTrunkTips;

        public const float DefaultTrunkTips = 0.6f;

        /// <summary>How far his hips would be down in all: <see cref="Sink"/>, and what his legs' reach to the nearest foot costs (metres).</summary>
        public float Sunk { get; private set; }

        private float leg, scale, height, omega;
        private int swing = -1;
        private Vector3 swingFrom, swingTo;
        private float swingFor, swingOver, swingLift, swingYawFrom, wait, sinkV;
        private readonly bool[] trails = new bool[2];
        private readonly float[] yawOff = new float[2];
        private Vector3 braked;
        private float already;
        private Vector3 began;
        private float beganFacing;

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        /// <summary>
        /// Is he off balance? The point where a foot would have to be to stop him (his body's place plus its speed
        /// times a third of a second or so) is outside what his feet hold. Inside it, his ankles and a step in his
        /// stance deal with it and no stumble begins.
        /// </summary>
        public static bool OffBalance(Vector3 com, Vector3 vel, Vector3 leftAnkle, Vector3 rightAnkle, float leg)
        {
            leg = Mathf.Max(0.3f, leg);
            float omega = Mathf.Sqrt(Gravity / (HeightOf * leg));
            Vector3 xi = Flat(com) + Flat(vel) / omega;
            Vector3 core = Nearest(Flat(leftAnkle), Flat(rightAnkle), xi);
            return (xi - core).magnitude > Sole * leg / 0.85f;
        }

        /// <summary>His weight's height over the ground, in leg lengths.</summary>
        public const float HeightOf = 1.15f;

        /// <summary>It begins: where his body is and how fast it goes now (with what the impact gave it), how he faces and turns, and where his feet are.</summary>
        /// <param name="alreadyGoing">How fast he was going already the way he is thrown (m/s): his feet were stepping at that pace, so they are that much faster to get ahead of him. A man run into as he runs stumbles on; a man standing is caught flat-footed.</param>
        public void Begin(Vector3 com, Vector3 vel, float facing, float spin, Vector3 leftAnkle, Vector3 rightAnkle, float leftYaw, float rightYaw, float legLength, float alreadyGoing = 0f)
        {
            already = Mathf.Max(0f, alreadyGoing);
            leg = Mathf.Max(0.3f, legLength);
            scale = leg / 0.85f;
            height = HeightOf * leg;
            omega = Mathf.Sqrt(Gravity / height);
            Com = Flat(com);
            Vel = Flat(vel);
            Facing = facing;
            Spin = Mathf.Clamp(spin, -MostSpin, MostSpin);
            Foot[0] = Flat(leftAnkle);
            Foot[1] = Flat(rightAnkle);
            Down[0] = Down[1] = true;
            trails[0] = trails[1] = false;
            FootYaw[0] = leftYaw;
            FootYaw[1] = rightYaw;
            yawOff[0] = Mathf.DeltaAngle(facing, leftYaw);
            yawOff[1] = Mathf.DeltaAngle(facing, rightYaw);
            swing = -1;
            wait = ReactsIn;
            Steps = 0;
            For = Went = Turned = 0f;
            Fastest = Vel.magnitude;
            Sink = sinkV = Sunk = 0f;
            Tilt = Vector3.zero;
            braked = Vector3.zero;
            began = Com;
            beganFacing = facing;
            Ended = Ends.None;
            Why = "";
            Active = true;
        }

        /// <summary>Something more throws him while he stumbles: a change of his speed (m/s, flat) and of his turning (degrees a second).</summary>
        public void Add(Vector3 change, float spin)
        {
            Vel += Flat(change);
            Spin = Mathf.Clamp(Spin + spin, -MostSpin, MostSpin);
            Fastest = Mathf.Max(Fastest, Vel.magnitude);
        }

        /// <summary>What the last footfalls took out of his speed (m/s, world, flat), handed over once: what rocks his upper body on as his hips are checked.</summary>
        public Vector3 TakeBraked()
        {
            Vector3 b = braked;
            braked = Vector3.zero;
            return b;
        }

        /// <summary>It is over (he is on his feet again, or the fall has him).</summary>
        public void End()
        {
            Active = false;
        }

        /// <summary>How far he has been carried from where it began (metres) and turned (degrees).</summary>
        public float Carried => (Com - began).magnitude;

        public float TurnedIn => Mathf.DeltaAngle(beganFacing, Facing);

        private static Vector3 Nearest(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 ab = b - a;
            float l = ab.sqrMagnitude;
            float t = l > 1e-8f ? Mathf.Clamp01(Vector3.Dot(p - a, ab) / l) : 0f;
            return a + ab * t;
        }

        /// <summary>The nearest point to <paramref name="p"/> that his feet on the ground can carry his weight at; false: none is down.</summary>
        private bool Held(Vector3 p, out Vector3 at)
        {
            Vector3 core;
            if (Down[0] && Down[1])
            {
                core = Nearest(Flat(Foot[0]), Flat(Foot[1]), p);
            }
            else if (Down[0] || Down[1])
            {
                core = Flat(Foot[Down[0] ? 0 : 1]);
            }
            else
            {
                at = p;
                return false;
            }

            at = core + Vector3.ClampMagnitude(p - core, Sole * scale);
            return true;
        }

        /// <summary>One step of his body over his feet: tipping from where his weight is carried, or going on as it goes with no foot down.</summary>
        private void Tip(ref Vector3 com, ref Vector3 vel, bool down0, bool down1, Vector3 foot0, Vector3 foot1, float dt)
        {
            if (down0 || down1)
            {
                Vector3 xi = com + vel / omega;
                Vector3 core = down0 && down1 ? Nearest(foot0, foot1, xi) : down0 ? foot0 : foot1;
                Vector3 mid = down0 && down1 ? 0.5f * (foot0 + foot1) : core;
                // (His ankles carry his weight as far toward stopping him as his soles reach, and a little past it.)
                Vector3 want = xi + 1.5f * (xi - mid);
                Vector3 nearest = down0 && down1 ? Nearest(foot0, foot1, want) : core;
                Vector3 cop = nearest + Vector3.ClampMagnitude(want - nearest, Sole * scale);
                vel += omega * omega * (com - cop) * dt;
            }

            com += vel * dt;
        }

        /// <summary>Where his body will be, and how fast, after <paramref name="seconds"/> with the swinging foot in the air.</summary>
        private void Ahead(float seconds, out Vector3 com, out Vector3 vel)
        {
            com = Com;
            vel = Vel;
            int s = 1 - swing;
            bool down = Down[s];
            Vector3 foot = Flat(Foot[s]);
            const float step = 1f / 60f;
            for (float t = 0f; t < seconds - 1e-4f; t += step)
            {
                float dt = Mathf.Min(step, seconds - t);
                Tip(ref com, ref vel, s == 0 && down, s == 1 && down, foot, foot, dt);
                down &= (com - foot).magnitude <= LeavesAt * leg;
            }
        }

        /// <summary>Where the swinging foot is to come down, landing in <paramref name="seconds"/>.</summary>
        private Vector3 Aim(float seconds)
        {
            Ahead(seconds, out Vector3 com, out Vector3 vel);
            Vector3 xi = com + vel / omega;
            Vector3 way = vel.sqrMagnitude > 1e-6f ? vel.normalized : (xi - com).sqrMagnitude > 1e-8f ? (xi - com).normalized : Vector3.zero;
            // (His own side of his body: his feet do not come down on one line.)
            Vector3 right = Quaternion.Euler(0f, Facing + Spin * seconds * 0.5f, 0f) * Vector3.right;
            Vector3 to = xi + way * (Past * scale) + right * ((swing == 0 ? -0.07f : 0.07f) * scale);
            Vector3 off = Vector3.ClampMagnitude(to - com, StepReach * leg);
            to = com + off;
            int s = 1 - swing;
            if (Down[s])
            {
                Vector3 apart = to - Flat(Foot[s]);
                if (apart.magnitude < Clear * scale)
                {
                    to = Flat(Foot[s]) + (apart.sqrMagnitude > 1e-6f ? apart.normalized : right * (swing == 0 ? -1f : 1f)) * (Clear * scale);
                }
            }

            return to;
        }

        private void Lift(int f)
        {
            swing = f;
            Down[f] = false;
            trails[f] = false;
            swingFrom = Flat(Foot[f]);
            swingYawFrom = FootYaw[f];
            swingFor = 0f;
            float fastest = FootSpeed * Mathf.Sqrt(scale) + already;
            swingOver = StepTakes;
            for (int i = 0; i < 4; i++)
            {
                swingTo = Aim(swingOver);
                float needs = (swingTo - swingFrom).magnitude / fastest;
                if (needs <= swingOver + 1e-3f)
                {
                    break;
                }

                swingOver = Mathf.Min(needs, 1.2f);
            }

            swingLift = (0.06f + 0.07f * Mathf.Clamp01((swingTo - swingFrom).magnitude / leg)) * scale;
        }

        private void Land(int f)
        {
            Foot[f] = swingTo;
            Down[f] = true;
            swing = -1;
            Steps++;
            wait = Between;
            FootYaw[f] = Facing + yawOff[f];
            // (The leg is a strut from the foot to his weight: what of his speed runs down it is lost into the ground.
            // A foot far ahead of him takes a great deal; one under him, nothing.)
            Vector3 strut = (Com - swingTo + Vector3.up * height).normalized;
            float along = Vector3.Dot(Vel, strut);
            if (along < 0f)
            {
                Vector3 took = -LegsBrake * along * Flat(strut);
                Vel += took;
                braked += took;
                // (And his knees give under it.)
                sinkV += KneesGive * 2f * Mathf.PI * KneesHz * -along;
            }

            // (A foot put down takes hold of the ground: half his turning goes into it at once.)
            Spin *= 0.5f;
        }

        /// <summary>How quickly his legs push his hips back up (Hz).</summary>
        private const float KneesHz = 2.2f;

        public void Step(float dt)
        {
            if (!Active || dt <= 0f)
            {
                return;
            }

            For += dt;
            Vector3 was = Com;
            bool down0 = Down[0], down1 = Down[1];
            Tip(ref Com, ref Vel, down0, down1, Flat(Foot[0]), Flat(Foot[1]), dt);
            Went += (Com - was).magnitude;
            Fastest = Mathf.Max(Fastest, Vel.magnitude);

            // His turning: the ground's grip on his soles takes it.
            float turned = Spin * dt;
            Facing += turned;
            Turned += Mathf.Abs(turned);
            Spin = Mathf.MoveTowards(Spin, 0f, (down0 && down1 ? GripBoth : down0 || down1 ? GripOne : 0f) * dt);

            // His hips: falling with no foot under him; pushed back up by a leg that stands.
            if (down0 || down1)
            {
                float w = 2f * Mathf.PI * KneesHz;
                sinkV += (-w * w * Sink - 2f * 0.9f * w * sinkV) * dt;
            }
            else
            {
                sinkV += Gravity * dt;
            }

            Sink += sinkV * dt;
            if (Sink < 0f)
            {
                Sink = 0f;
                sinkV = Mathf.Max(0f, sinkV);
            }

            // A foot his body has left behind comes off the ground by itself and is drawn after him.
            for (int f = 0; f < 2; f++)
            {
                Vector3 from = Flat(Foot[f]) - Com;
                if (Down[f] && from.magnitude > LeavesAt * leg)
                {
                    Down[f] = false;
                    trails[f] = true;
                }

                if (trails[f])
                {
                    Vector3 drawn = Com + Vector3.ClampMagnitude(from, LeavesAt * leg);
                    Foot[f] = new Vector3(drawn.x, Mathf.MoveTowards(Foot[f].y, 0.05f * scale, 0.6f * dt), drawn.z);
                }
            }

            // His steps.
            if (swing >= 0)
            {
                swingFor += dt;
                float u = Mathf.Clamp01(swingFor / Mathf.Max(0.05f, swingOver));
                if (u < 0.7f)
                {
                    // (Aimed as it goes: where it must land moves with him.)
                    swingTo = Aim(swingOver - swingFor);
                }

                if (u >= 1f)
                {
                    Land(swing);
                }
                else
                {
                    float over = u * u * (3f - 2f * u);
                    Vector3 at = Vector3.Lerp(swingFrom, swingTo, over);
                    Foot[swing] = new Vector3(at.x, 4f * u * (1f - u) * swingLift, at.z);
                    FootYaw[swing] = Mathf.LerpAngle(swingYawFrom, Facing + yawOff[swing], over);
                }
            }

            if (swing < 0)
            {
                wait -= dt;
                Vector3 xi = Com + Vel / omega;
                bool held = Held(xi, out Vector3 at);
                bool inside = held && (xi - at).magnitude < 1e-3f;
                if (trails[0] || trails[1])
                {
                    // (A foot left behind in the air: it is the next to step.)
                    Lift(trails[0] && trails[1] ? Behind() : trails[0] ? 0 : 1);
                }
                else if (wait <= 0f && !inside)
                {
                    Lift(Behind());
                }
            }

            // How far down his hips are for his legs to reach a foot.
            float nearest = float.MaxValue;
            for (int f = 0; f < 2; f++)
            {
                if (Down[f])
                {
                    nearest = Mathf.Min(nearest, (Flat(Foot[f]) - Com).magnitude);
                }
            }

            float reach = nearest == float.MaxValue ? 0f : leg - Mathf.Sqrt(Mathf.Max(0f, leg * leg - Mathf.Min(nearest, leg) * Mathf.Min(nearest, leg)));
            Sunk = Sink + reach;
            Vector3 carried;
            if (!Held(Com, out carried))
            {
                carried = 0.5f * (Flat(Foot[0]) + Flat(Foot[1]));
            }

            Vector3 past = Com - carried;
            Tilt = past.sqrMagnitude > 1e-8f ? past.normalized * Mathf.Atan2(past.magnitude, height) : Vector3.zero;

            // How it ends.
            float speed = Vel.magnitude;
            if (Sunk > DownAt * leg)
            {
                Why = Down[0] || Down[1]
                    ? $"his body {nearest:0.00} m past the foot that carried it"
                    : $"no foot under him for too long (his hips {Sink:0.00} m down)";
                Ended = Ends.Down;
            }
            else if (For > 4f || Steps > 12)
            {
                Why = "stumbling on and on";
                Ended = speed > 0.8f * Mathf.Sqrt(scale) ? Ends.Down : Ends.Caught;
            }
            else if (swing < 0 && Down[0] && Down[1] && speed < CaughtBelow * Mathf.Sqrt(scale) && Mathf.Abs(Spin) < 90f && Sink < 0.04f * scale)
            {
                Vector3 xi = Com + Vel / omega;
                Held(xi, out Vector3 at);
                if ((xi - at).magnitude < 1e-3f)
                {
                    Ended = Ends.Caught;
                }
            }

            if (Ended != Ends.None)
            {
                Active = false;
            }
        }

        /// <summary>The foot behind, the way he is going: the one that steps.</summary>
        private int Behind()
        {
            Vector3 way = Vel.sqrMagnitude > 1e-6f ? Vel.normalized : Vector3.forward;
            return Vector3.Dot(Flat(Foot[0]) - Com, way) <= Vector3.Dot(Flat(Foot[1]) - Com, way) ? 0 : 1;
        }

        /// <summary>In words, for the log: what the stumble was.</summary>
        public string Told()
        {
            var c = System.Globalization.CultureInfo.InvariantCulture;
            return string.Format(c, "{0} stumble step{1} over {2:0.00} m in {3:0.00} s, turned {4:0} degrees, at {5:0.0} m/s at its fastest", Steps, Steps == 1 ? "" : "s", Carried, For, TurnedIn, Fastest);
        }
    }
}
