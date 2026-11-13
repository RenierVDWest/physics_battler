using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// Stand and step (the plan "Grounded legs", step 1). The fight steers a fighter by a velocity, and near its enemy
    /// that velocity is small nearly all the time: holding a distance, going round at a third of a metre a second. Walked
    /// as asked, that is a walk in slow motion: a foot hanging in the air for a second while the other carries a body
    /// that is not over it. Paced, the fighter stands, planted, until the steering has asked for a step's worth of
    /// ground; then it goes the way it is asked at a step's pace, until the steering stops asking that way or the
    /// ground asked for is covered; and stands again. A velocity as fast as a walk is walked as it is.
    ///
    /// Between the steering and the walk (<see cref="ClipFighter.Move"/>): the steering's velocity in, the walk's out.
    /// How the step is taken is the walk's: in its guard, standing, the feet step after the body in the stance
    /// (<see cref="Locomotor.StepsInStance"/>); else the guard's walk is walked at a step's rhythm.
    /// </summary>
    public sealed class StepPacer
    {
        /// <summary>Paced at all (the scene's Combat Feel, "Stand And Step"); off, the steering's velocity is walked as it is.</summary>
        public static bool On = true;

        /// <summary>How much ground the steering has to have asked for before a step is taken (metres at a Medium's size).</summary>
        public static float StepWorth = DefaultStepWorth;

        public const float DefaultStepWorth = 0.22f;

        /// <summary>The pace it steps at (m/s at a Medium's size): the walk it asks of the guard's clips while it steps.</summary>
        public static float StepPace = DefaultStepPace;

        public const float DefaultStepPace = 0.75f;

        /// <summary>A velocity asked this fast is a walk, and walked as it is (m/s at a Medium's size): the guard's walks are recorded at 1.1 to 1.5.</summary>
        public const float WalkFrom = 0.9f;

        /// <summary>Standing, what was asked for is forgotten over this long (seconds): a velocity too small to add up to a step in that time never makes one.</summary>
        public const float Forgets = 1.5f;

        /// <summary>The least it stands between two steps (seconds), and the longest one spell of stepping lasts.</summary>
        public const float StandsLeast = 0.2f, StepsMost = 4f;

        /// <summary>The ground asked for and not yet covered (metres, flat).</summary>
        public Vector3 Owed { get; private set; }

        /// <summary>Stepping now (or walking as asked); standing otherwise.</summary>
        public bool Stepping { get; private set; }

        /// <summary>Walking as asked: the velocity asked is as fast as a walk.</summary>
        public bool Walking { get; private set; }

        /// <summary>For the reports: spells of stepping begun, those that went back on the one before within two seconds, and seconds stood and stepped.</summary>
        public int Spells { get; private set; }

        public int Reversals { get; private set; }

        public float Stood { get; private set; }

        public float Stepped { get; private set; }

        private Vector3 way, lastWay;
        private float stood = 10f, stepping, notAsked, sinceLast = 10f;

        /// <summary>Forgets what was asked: a clip took the body, it fell, it was put somewhere.</summary>
        public void Forget()
        {
            Owed = Vector3.zero;
            Stepping = false;
            Walking = false;
            stood = 10f;
        }

        /// <summary>
        /// One step's pacing: the velocity the steering asks (<paramref name="want"/>, m/s, flat) and how far the walk
        /// moved the body since the last (<paramref name="moved"/>), to the velocity asked of the walk.
        /// </summary>
        /// <param name="coasts">How far the walk goes on once it is asked to stop, for each m/s it is going (seconds): a step is ended that much early.</param>
        /// <param name="free">Free to begin a step: not while a shove is still being walked off.</param>
        public Vector3 Pace(float dt, Vector3 want, Vector3 moved, float size, float coasts, bool free = true)
        {
            want.y = 0f;
            moved.y = 0f;
            if (!On)
            {
                Forget();
                return want;
            }

            float root = Mathf.Sqrt(size), asked = want.magnitude;
            sinceLast += dt;
            if (asked >= WalkFrom * root)
            {
                // A walk: as it is asked. (Slowing out of it, it is a step under way: the body goes on to where it was
                // going instead of stopping to start again.)
                Walking = true;
                Stepping = true;
                Owed = want.normalized * (StepWorth * size);
                way = want / asked;
                stepping = 0f;
                notAsked = 0f;
                Stepped += dt;
                return want;
            }

            Walking = false;
            Owed += want * dt;
            if (Stepping)
            {
                Owed -= moved;
                stepping += dt;
                Stepped += dt;
                float left = Vector3.Dot(Owed, way);
                // (Still asked that way: a steering that has what it wanted asks nothing, or the other way.)
                notAsked = Vector3.Dot(want, way) > 0.05f * root ? 0f : notAsked + dt;
                // (No slower than it is asked: a steady slow walk is stepped at its own speed, in short steps.)
                float pace = Mathf.Max(StepPace * root, asked);
                // (Ended as far short as the walk goes on by itself, half a step's worth at the most: a walk that
                // coasts further than a step is long would end every step as it began.)
                if (left <= Mathf.Min(pace * coasts, 0.5f * StepWorth * size) || notAsked > 0.08f || stepping > StepsMost)
                {
                    Stepping = false;
                    Owed = Vector3.zero;
                    stood = 0f;
                    lastWay = way;
                    sinceLast = 0f;
                    return Vector3.zero;
                }

                // (The way it steps follows what is owed: a circle is stepped round, not off along its tangent.)
                Vector3 to = Owed.sqrMagnitude > 1e-6f ? Owed.normalized : way;
                way = Vector3.Slerp(way, to, 1f - Mathf.Exp(-dt / 0.1f)).normalized;
                return way * pace;
            }

            stood += dt;
            Stood += dt;
            Owed *= Mathf.Exp(-dt / Forgets);
            if (Owed.magnitude >= StepWorth * size && stood >= StandsLeast && free)
            {
                Stepping = true;
                stepping = 0f;
                notAsked = 0f;
                way = Owed.normalized;
                Spells++;
                if (sinceLast < 2f && Vector3.Dot(way, lastWay) < -0.5f)
                {
                    Reversals++;
                }

                return way * (StepPace * root);
            }

            return Vector3.zero;
        }
    }
}
