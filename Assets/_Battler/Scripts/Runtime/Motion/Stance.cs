using System;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Where the feet are. Each foot is planted on the ground and stays put until it is too far from
    /// its spot in the stance (the unit moved or turned), then it steps: lifts, swings along an arc
    /// and plants again. One foot at a time. Idle shuffles move a foot a little inside its spot.
    ///
    /// Spots are in the unit's space: the lead foot forward and to its side, the rear foot back,
    /// toes turned out. Positions are ground points (y = ground height); the ankle sits above them.
    /// Pure maths, no Unity objects, so it is tested on its own.
    /// </summary>
    public sealed class Stance
    {
        public const int Left = 0;
        public const int Right = 1;

        internal struct Foot
        {
            public Vector3 position;
            public float yaw;
            public bool stepping;
            public Vector3 from, to;
            public float fromYaw, toYaw;
            public float t;
            public float lift;
            public float duration;   // this step's length in seconds (0: the stance's own)
            public float height;     // how high this step lifts, metres (0: the stance's own)
            public float carry;      // the lift it had when a step under way was redirected (fades out)
        }

        /// <summary>A copy of the feet and timers, to try a pose and put everything back (the move planner).</summary>
        public struct Snapshot
        {
            internal Foot left, right;
            internal float shuffleTimer, restAfterStep;
            internal int nextShuffleFoot, steps, shuffles;
            internal bool hold;
        }

        private readonly Foot[] feet = new Foot[2];
        private readonly System.Random random;
        private float width, length, threshold, turnThreshold, duration, height, shuffleDistance, shuffleInterval;
        private float leadToeOut, rearToeOut;
        private bool leadLeft = true;
        private float shuffleTimer;
        private float restAfterStep;
        private int nextShuffleFoot;

        public event Action<int, Vector3> Landed;

        /// <summary>While held, the feet only take the steps they are told to (<see cref="StepTo"/>): a move is placing them.</summary>
        public bool Hold { get; set; }

        public int StepsTaken { get; private set; }
        public int ShufflesTaken { get; private set; }
        public float GroundY { get; set; }

        public Stance(int seed = 1)
        {
            random = new System.Random(seed);
        }

        public bool LeadLeft => leadLeft;

        public void Configure(CombatStyle style, BodyMeasure body)
        {
            float leg = body.LegLength;
            width = style.stanceWidth * leg;
            length = style.stanceLength * leg;
            threshold = style.stepThreshold * leg;
            turnThreshold = style.stepTurnThreshold;
            duration = style.stepDuration * Mathf.Sqrt(Mathf.Max(0.2f, body.SizeFactor));
            height = style.stepHeight * leg;
            shuffleDistance = style.shuffleDistance * leg;
            shuffleInterval = style.shuffleInterval;
            leadLeft = style.leadWithLeftFoot;
            leadToeOut = style.leadToeOut;
            rearToeOut = style.rearToeOut;
            shuffleTimer = shuffleInterval * (0.5f + (float)random.NextDouble());
        }

        public bool IsLead(int foot) => (foot == Left) == leadLeft;

        /// <summary>The foot's spot in the stance, in the unit's space (ground level).</summary>
        public Vector3 HomeLocal(int foot)
        {
            float x = foot == Left ? -width * 0.5f : width * 0.5f;
            float z = IsLead(foot) ? length * 0.5f : -length * 0.5f;
            return new Vector3(x, 0f, z);
        }

        /// <summary>The foot's turn in the stance, degrees about up (toes out).</summary>
        public float HomeYawLocal(int foot)
        {
            float toeOut = IsLead(foot) ? leadToeOut : rearToeOut;
            return foot == Left ? -toeOut : toeOut;
        }

        public Vector3 HomeWorld(int foot, Vector3 rootPosition, float rootYaw)
        {
            var p = rootPosition + Quaternion.Euler(0f, rootYaw, 0f) * HomeLocal(foot);
            p.y = GroundY;
            return p;
        }

        public void Plant(Vector3 rootPosition, float rootYaw)
        {
            for (int f = 0; f < 2; f++)
            {
                feet[f].position = HomeWorld(f, rootPosition, rootYaw);
                feet[f].yaw = rootYaw + HomeYawLocal(f);
                feet[f].stepping = false;
                feet[f].lift = 0f;
            }
        }

        public Vector3 Position(int foot) => feet[foot].position;

        /// <summary>Seconds until a stepping foot lands (0 if it stands).</summary>
        public float StepTimeLeft(int foot) => feet[foot].stepping ? (1f - feet[foot].t) * (feet[foot].duration > 0f ? feet[foot].duration : duration) : 0f;

        /// <summary>Where a foot is going (where it stands, if it isn't stepping).</summary>
        public Vector3 Target(int foot) => feet[foot].stepping ? feet[foot].to : feet[foot].position;
        public float Yaw(int foot) => feet[foot].yaw;
        public float Lift(int foot) => feet[foot].lift;
        public bool Stepping(int foot) => feet[foot].stepping;
        public bool AnyStepping => feet[0].stepping || feet[1].stepping;

        /// <summary>Progress through the current step (0 to 1), or 0 when planted.</summary>
        public float StepProgress(int foot) => feet[foot].stepping ? feet[foot].t : 0f;

        /// <summary>How far a planted foot is from its spot (horizontal metres).</summary>
        public float Error(int foot, Vector3 rootPosition, float rootYaw)
        {
            var d = feet[foot].position - HomeWorld(foot, rootPosition, rootYaw);
            d.y = 0f;
            return d.magnitude;
        }

        /// <summary>
        /// How the unit is being moved by something else (a knock-back, m/s): the feet hurry after
        /// it (quicker steps, no pause between them) and step to where the body will be, not where
        /// it is.
        /// </summary>
        public Vector3 Drift { get; set; }

        /// <summary>How urgently the feet must keep up (0 to 1): off balance, a foot steps when it is only a little behind.</summary>
        public float Urgency { get; set; }

        public void Update(Vector3 rootPosition, float rootYaw, float dt)
        {
            for (int f = 0; f < 2; f++)
            {
                if (feet[f].stepping)
                {
                    Advance(f, dt);
                }
            }

            if (AnyStepping || Hold)
            {
                return;
            }

            float hurry = Mathf.Clamp01(Drift.magnitude / 1.5f);
            if (restAfterStep > 0f && hurry < 0.2f)
            {
                restAfterStep -= dt;
                return;
            }

            // The foot furthest from its spot steps first, when one is too far or turned too much.
            // Off balance (a stumble), a foot steps as soon as it is a little behind: catch steps.
            float near = threshold * (1f - 0.6f * Mathf.Clamp01(Urgency));
            int worst = -1;
            float worstScore = 1f;
            for (int f = 0; f < 2; f++)
            {
                float distance = Error(f, rootPosition, rootYaw);
                float turn = Mathf.Abs(Mathf.DeltaAngle(feet[f].yaw, rootYaw + HomeYawLocal(f)));
                float score = Mathf.Max(near > 0f ? distance / near : 0f, turnThreshold > 0f ? turn / turnThreshold : 0f);
                if (score > worstScore)
                {
                    worstScore = score;
                    worst = f;
                }
            }

            if (worst >= 0)
            {
                float seconds = duration * (1f - 0.45f * hurry);
                Begin(worst, HomeWorld(worst, rootPosition + Drift * seconds, rootYaw), rootYaw + HomeYawLocal(worst));
                feet[worst].duration = seconds;
                StepsTaken++;
                return;
            }

            if (shuffleInterval > 0f)
            {
                shuffleTimer -= dt;
                if (shuffleTimer <= 0f)
                {
                    shuffleTimer = shuffleInterval * (0.6f + 0.8f * (float)random.NextDouble());
                    int f = nextShuffleFoot;
                    nextShuffleFoot = 1 - nextShuffleFoot;
                    float angle = (float)(random.NextDouble() * Math.PI * 2.0);
                    float reach = shuffleDistance * (0.5f + 0.5f * (float)random.NextDouble());
                    var offset = Quaternion.Euler(0f, rootYaw, 0f) * new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * reach;
                    Begin(f, HomeWorld(f, rootPosition, rootYaw) + offset, rootYaw + HomeYawLocal(f));
                    ShufflesTaken++;
                }
            }
        }

        /// <summary>
        /// Step a foot to a ground point now, taking the given time (a lunge's planned step). A foot
        /// already stepping is redirected from where it is.
        /// </summary>
        public void StepTo(int foot, Vector3 to, float toYaw, float seconds, float lift = 0f)
        {
            to.y = GroundY;
            Begin(foot, to, toYaw);
            feet[foot].duration = Mathf.Max(0.05f, seconds);
            feet[foot].height = Mathf.Max(0f, lift);
            StepsTaken++;
        }

        /// <summary>Put a foot down at a point at once (planning: where a planned step will land).</summary>
        public void SetFoot(int foot, Vector3 position, float yaw)
        {
            position.y = GroundY;
            feet[foot].position = position;
            feet[foot].yaw = yaw;
            feet[foot].stepping = false;
            feet[foot].lift = 0f;
        }

        public Snapshot Save() => new Snapshot
        {
            left = feet[Left], right = feet[Right], shuffleTimer = shuffleTimer, restAfterStep = restAfterStep,
            nextShuffleFoot = nextShuffleFoot, steps = StepsTaken, shuffles = ShufflesTaken, hold = Hold,
        };

        public void Restore(Snapshot snapshot)
        {
            feet[Left] = snapshot.left;
            feet[Right] = snapshot.right;
            shuffleTimer = snapshot.shuffleTimer;
            restAfterStep = snapshot.restAfterStep;
            nextShuffleFoot = snapshot.nextShuffleFoot;
            StepsTaken = snapshot.steps;
            ShufflesTaken = snapshot.shuffles;
            Hold = snapshot.hold;
        }

        /// <summary>The lead foot (the one forward in the stance).</summary>
        public int Lead => leadLeft ? Left : Right;

        private void Begin(int f, Vector3 to, float toYaw)
        {
            // (Redirected in the air: it doesn't drop to the ground to start again.)
            feet[f].carry = feet[f].stepping ? feet[f].lift : 0f;
            feet[f].duration = 0f;
            feet[f].height = 0f;
            feet[f].stepping = true;
            feet[f].from = feet[f].position;
            feet[f].to = to;
            feet[f].fromYaw = feet[f].yaw;
            feet[f].toYaw = toYaw;
            feet[f].t = 0f;
        }

        private void Advance(int f, float dt)
        {
            ref var foot = ref feet[f];
            foot.t = Mathf.Min(1f, foot.t + dt / Mathf.Max(0.05f, foot.duration > 0f ? foot.duration : duration));
            float s = foot.t * foot.t * (3f - 2f * foot.t);
            foot.position = Vector3.Lerp(foot.from, foot.to, s);
            foot.position.y = GroundY;
            foot.yaw = foot.fromYaw + Mathf.DeltaAngle(foot.fromYaw, foot.toYaw) * s;
            float distance = Vector3.Distance(foot.from, foot.to);
            foot.lift = foot.height > 0f
                ? foot.height * Mathf.Sin(Mathf.PI * foot.t)
                : height * Mathf.Clamp(distance / Mathf.Max(1e-3f, threshold), 0.4f, 1f) * Mathf.Sin(Mathf.PI * foot.t);
            foot.lift = Mathf.Max(foot.lift, foot.carry * (1f - s));
            if (foot.t >= 1f)
            {
                foot.stepping = false;
                foot.lift = 0f;
                restAfterStep = duration * 0.25f;
                Landed?.Invoke(f, foot.position);
            }
        }
    }
}
