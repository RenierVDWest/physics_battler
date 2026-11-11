using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Plays a walk cycle (<see cref="GaitClip"/>) while the unit walks: fades in with the walking
    /// speed, runs round the cycle at the capture's cadence (a little slower for a slower walk, and
    /// the steps shorter), says when each foot lifts, and gives the body's motion for the pose. The
    /// unit takes the steps (it knows where the ground is going).
    /// </summary>
    public sealed class GaitPlayer
    {
        public GaitClip Clip { get; }

        /// <summary>How much the walk is playing (0: standing, 1: walking).</summary>
        public float Weight { get; private set; }

        /// <summary>Round the cycle, 0 to 1.</summary>
        public float Phase { get; private set; }

        /// <summary>Seconds for a cycle at the walk's speed now.</summary>
        public float CycleTime { get; private set; } = 1f;

        /// <summary>The walking velocity (smoothed, world, metres a second).</summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>Still walking (not slowing to a stop): it may take new steps.</summary>
        public bool Walking { get; private set; }

        /// <summary>Feet to lift this update.</summary>
        public readonly List<int> DueSteps = new List<int>();

        private float pace = 1f;

        public GaitPlayer(GaitClip clip)
        {
            Clip = clip;
        }

        public bool IsEmpty => Clip == null || Clip.IsEmpty;

        /// <param name="velocity">How fast the unit's root is being walked (world).</param>
        /// <param name="free">On its feet with nothing else moving its legs (no blow, no knock-back).</param>
        /// <param name="nominal">The style's walking speed on this body, m/s.</param>
        public void Update(float dt, Vector3 velocity, bool free, float nominal, float leg)
        {
            DueSteps.Clear();
            if (IsEmpty)
            {
                Weight = 0f;
                return;
            }

            if (dt <= 0f)
            {
                return;   // (a hit stop: the walk holds where it is)
            }

            velocity.y = 0f;
            Velocity = Vector3.Lerp(Velocity, velocity, 1f - Mathf.Exp(-dt / 0.1f));
            float speed = Velocity.magnitude;
            nominal = Mathf.Max(0.05f, nominal);
            float wanted = free ? Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.25f, 0.55f, speed / nominal)) : 0f;
            Walking = wanted > 0.5f;
            Weight = Mathf.MoveTowards(Weight, wanted, dt / (wanted > Weight ? 0.15f : 0.25f));
            if (Weight <= 0f)
            {
                return;
            }

            // The capture's cadence, slowed for a slower walk by the square root of the speed (the
            // steps shorten for the rest), never below 60% of the walk's own speed.
            float cycle = Clip.length * pace;
            float clipSpeed = Clip.distance * leg / cycle;
            float ratio = Mathf.Clamp(Mathf.Max(speed, 0.6f * nominal) / clipSpeed, 0.25f, 1.5f);
            CycleTime = cycle / Mathf.Sqrt(ratio);
            // No slower than lets a planted foot pass under the hips within reach (about 0.7 of a
            // leg from a third ahead to behind) while it stands.
            float stanceShare = 1f - 0.5f * (Clip.Swing(Stance.Left) + Clip.Swing(Stance.Right));
            CycleTime = Mathf.Min(CycleTime, 0.7f * leg / Mathf.Max(1e-3f, speed * stanceShare));
            float before = Phase;
            Phase = Mathf.Repeat(Phase + dt / CycleTime, 1f);
            // (Coming to a stop, no new steps: the stance takes the feet home from where they are.)
            if (Weight < 0.3f || !Walking)
            {
                return;
            }

            for (int f = 0; f < 2; f++)
            {
                if (Crossed(before, Phase, Clip.Lift(f)))
                {
                    DueSteps.Add(f);
                }
            }
        }

        /// <summary>Stops at once (knocked down, moved by the sandbox).</summary>
        public void Stop()
        {
            Weight = 0f;
            Walking = false;
            Velocity = Vector3.zero;
            DueSteps.Clear();
        }

        private static bool Crossed(float from, float to, float at)
        {
            return from <= to ? at > from && at <= to : at > from || at <= to;
        }
    }
}
