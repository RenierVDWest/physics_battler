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

        private struct Foot
        {
            public Vector3 position;
            public float yaw;
            public bool stepping;
            public Vector3 from, to;
            public float fromYaw, toYaw;
            public float t;
            public float lift;
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

        public void Update(Vector3 rootPosition, float rootYaw, float dt)
        {
            for (int f = 0; f < 2; f++)
            {
                if (feet[f].stepping)
                {
                    Advance(f, dt);
                }
            }

            if (AnyStepping)
            {
                return;
            }

            if (restAfterStep > 0f)
            {
                restAfterStep -= dt;
                return;
            }

            // The foot furthest from its spot steps first, when one is too far or turned too much.
            int worst = -1;
            float worstScore = 1f;
            for (int f = 0; f < 2; f++)
            {
                float distance = Error(f, rootPosition, rootYaw);
                float turn = Mathf.Abs(Mathf.DeltaAngle(feet[f].yaw, rootYaw + HomeYawLocal(f)));
                float score = Mathf.Max(threshold > 0f ? distance / threshold : 0f, turnThreshold > 0f ? turn / turnThreshold : 0f);
                if (score > worstScore)
                {
                    worstScore = score;
                    worst = f;
                }
            }

            if (worst >= 0)
            {
                Begin(worst, HomeWorld(worst, rootPosition, rootYaw), rootYaw + HomeYawLocal(worst));
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

        private void Begin(int f, Vector3 to, float toYaw)
        {
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
            foot.t = Mathf.Min(1f, foot.t + dt / Mathf.Max(0.05f, duration));
            float s = foot.t * foot.t * (3f - 2f * foot.t);
            foot.position = Vector3.Lerp(foot.from, foot.to, s);
            foot.position.y = GroundY;
            foot.yaw = foot.fromYaw + Mathf.DeltaAngle(foot.fromYaw, foot.toYaw) * s;
            float distance = Vector3.Distance(foot.from, foot.to);
            foot.lift = height * Mathf.Clamp(distance / Mathf.Max(1e-3f, threshold), 0.4f, 1f) * Mathf.Sin(Mathf.PI * foot.t);
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
