using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>
    /// The timeline of a Swing or Thrust move, worked out from its numbers rather than drawn as
    /// curves. Five phases (medium size; units scale the whole move with their size):
    ///
    ///  1. Wind-up: the weapon is drawn back from the guard to the loaded pose, easing in and out.
    ///  2. Hold: a tense moment at the top, creeping a little further back.
    ///  3. Strike: from the release to the contact, accelerating over the first part, then flat out
    ///     (a parabola, then a constant-speed arc, as measured cuts are).
    ///  4. Follow-through: braking evenly from that speed to a stop; its length follows from the speed.
    ///  5. Recovery: the move blends back into the guard.
    ///
    /// Progress along the path is measured in what the weapon travels (degrees of arc for a swing,
    /// metres for a thrust), so the path can be laid out evenly in it. The body (lunge, turn, lean,
    /// crouch) follows the same phases, the hips leading the arm and the chest between.
    /// </summary>
    public partial class MoveDefinition
    {
        /// <summary>How far the hips and chest turn ahead of the weapon (seconds at medium size).</summary>
        public const float HipLead = 0.08f, ChestLead = 0.04f;

        /// <summary>Share of the turn the hips take; the chest turns the rest on top.</summary>
        public const float HipShare = 0.45f;

        private const float SwingCreep = 4f, ThrustCreep = 0.03f;
        private const float ThrustWindUp = 0.45f, ThrustStrike = 0.5f;

        /// <summary>What the weapon travels in a phase (0 wind-up, 1 hold, 2 strike, 3 follow-through): degrees for a swing, metres for a thrust.</summary>
        public float SwingTravel(int phase)
        {
            bool thrust = shape == MoveShape.Thrust;
            switch (phase)
            {
                case 0: return thrust ? ThrustWindUp : Mathf.Max(10f, windUpAngle - guardAngle);
                case 1: return thrust ? ThrustCreep : SwingCreep;
                case 2: return thrust ? ThrustStrike : windUpAngle + SwingCreep;
                default: return thrust ? Mathf.Max(0.05f, through - drive) : Mathf.Max(5f, followAngle);
            }
        }

        public float SwingTotal => SwingTravel(0) + SwingTravel(1) + SwingTravel(2) + SwingTravel(3);

        /// <summary>Speed through contact in travel units a second (medium size).</summary>
        public float SwingPeakRate => SwingTravel(2) * StrikeGain / Mathf.Max(0.02f, strikeTime);

        /// <summary>The strike's top speed against its mean: accelerating over a share of it, then constant.</summary>
        private float StrikeGain => 1f / (1f - 0.5f * Mathf.Clamp(accelShare, 0.05f, 0.95f));

        /// <summary>Seconds of follow-through: braking evenly from the strike's speed.</summary>
        public float FollowTime => 2f * SwingTravel(3) / Mathf.Max(1e-3f, SwingPeakRate);

        public float SwingDuration => Mathf.Max(0.05f, windUpTime) + Mathf.Max(0f, holdTime) + Mathf.Max(0.02f, strikeTime) + FollowTime
                                     + Mathf.Max(0.05f, recoveryTime);

        private float T1 => Mathf.Max(0.05f, windUpTime);
        private float T2 => T1 + Mathf.Max(0f, holdTime);
        private float T3 => T2 + Mathf.Max(0.02f, strikeTime);
        private float T4 => T3 + FollowTime;

        public float SwingContactTime => T3 / SwingDuration;

        /// <summary>When the wind-up ends (normalized time): a feint plays the move up to a share of this and back.</summary>
        public float WindUpEnd => shape == MoveShape.Path ? 0.3f : T1 / SwingDuration;

        /// <summary>When the follow-through ends and the recovery starts (normalized time).</summary>
        public float SwingFollowEnd => shape == MoveShape.Path ? 1f : T4 / SwingDuration;

        public float SwingContactProgress => (SwingTravel(0) + SwingTravel(1) + SwingTravel(2)) / SwingTotal;

        private static float Smooth(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }

        /// <summary>Travel so far at a moment (seconds), in the move's units.</summary>
        private float TravelAt(float s)
        {
            float a0 = SwingTravel(0), a1 = SwingTravel(1), a2 = SwingTravel(2), a3 = SwingTravel(3);
            if (s <= T1)
            {
                return a0 * Smooth(s / T1);
            }

            if (s <= T2)
            {
                return a0 + a1 * Smooth((s - T1) / Mathf.Max(1e-4f, T2 - T1));
            }

            if (s <= T3)
            {
                float u = (s - T2) / (T3 - T2);
                float f = Mathf.Clamp(accelShare, 0.05f, 0.95f);
                float v = StrikeGain;
                float x = u < f ? v * u * u / (2f * f) : v * (0.5f * f + u - f);
                return a0 + a1 + a2 * Mathf.Clamp01(x);
            }

            if (s <= T4)
            {
                float u = (s - T3) / Mathf.Max(1e-4f, T4 - T3);
                return a0 + a1 + a2 + a3 * (2f * u - u * u);
            }

            // The recovery comes back round the front, part of the way along the follow-through, as
            // it blends into the guard (a straight blend from a follow-through wrapped round the body
            // would pass through it).
            float back = (s - T4) / Mathf.Max(0.05f, recoveryTime);
            return a0 + a1 + a2 + a3 * (1f - RecoverBack * Smooth(back / 0.6f));
        }

        /// <summary>Share of the follow-through the recovery comes back along.</summary>
        private const float RecoverBack = 0.45f;

        private float SwingProgress(float t) => Mathf.Clamp01(TravelAt(t * SwingDuration) / SwingTotal);

        private float SwingBlend(float t)
        {
            float s = t * SwingDuration;
            if (s <= T4)
            {
                return Smooth(s / (0.3f * T1));   // (taken over before the weapon turns far from the guard: a blend between two far-apart turns would flip)
            }

            // Held while the weapon comes back round the front, then let go into the guard.
            return 1f - Smooth(((s - T4) / Mathf.Max(0.05f, recoveryTime) - 0.35f) / 0.65f);
        }

        /// <summary>A value through the phases: 0, then <paramref name="load"/> by the end of the wind-up (held), then <paramref name="strike"/> at contact (held through the follow-through), then back to 0.</summary>
        private float Phased(float t, float load, float strike, float loadBy = 1f)
        {
            float s = t * SwingDuration;
            float loaded = loadBy * T1;
            if (s <= loaded)
            {
                return load * Smooth(s / loaded);
            }

            if (s <= T3)
            {
                return Mathf.Lerp(load, strike, Smooth((s - Mathf.Max(loaded, T2 - 0.5f * (T2 - T1))) / Mathf.Max(1e-3f, T3 - Mathf.Max(loaded, T2 - 0.5f * (T2 - T1)))));
            }

            if (s <= T4)
            {
                return strike;
            }

            return strike * (1f - Smooth((s - T4) / Mathf.Max(0.05f, recoveryTime)));
        }

        /// <summary>
        /// The lunge (0 to 1 at contact, below 0 rocked back): back through most of the wind-up, then
        /// driving forward from late in the wind-up to land whole at contact (the step goes with it).
        /// </summary>
        private float SwingBody(float t)
        {
            float s = t * SwingDuration;
            float low = 0.75f * T1;
            if (s <= low)
            {
                return -rockBack * Smooth(s / low);
            }

            if (s <= T3)
            {
                return Mathf.Lerp(-rockBack, 1f, Smooth((s - low) / (T3 - low)));
            }

            if (s <= T4)
            {
                return 1f;
            }

            return 1f - Smooth((s - T4) / Mathf.Max(0.05f, recoveryTime));
        }

        /// <summary>Metres (medium size) the body has driven on past its lunge since contact (thrusts).</summary>
        public float DriveAt(float t)
        {
            if (shape == MoveShape.Path || drive <= 0f)
            {
                return 0f;
            }

            float s = t * SwingDuration;
            if (s <= T3)
            {
                return 0f;
            }

            if (s <= T4)
            {
                return drive * Smooth((s - T3) / Mathf.Max(1e-3f, T4 - T3));
            }

            return drive * (1f - Smooth((s - T4) / Mathf.Max(0.05f, recoveryTime)));
        }

        /// <summary>Degrees of lean (forward +) at a moment.</summary>
        public float LeanAt(float t) => Phased(t, leanBack, leanThrough);

        /// <summary>Hips lowered at a moment, share of the leg.</summary>
        public float CrouchAt(float t) => Phased(t, crouchLoad, crouchStrike);

        /// <summary>
        /// The hips and chest's total turn at a moment (degrees, + turned right): away in the wind-up
        /// with the weapon, then through with the swing, then back to square.
        /// </summary>
        public float TurnAt(float t)
        {
            if (shape == MoveShape.Path)
            {
                return 0f;
            }

            float s = Mathf.Clamp01(t) * SwingDuration;
            float sign = backhand ? -1f : 1f;
            float a = TravelAt(s);
            float a0 = SwingTravel(0) + SwingTravel(1);
            float turn;
            if (a <= a0)
            {
                turn = turnBack * a / Mathf.Max(1e-4f, a0);
            }
            else
            {
                float u = (a - a0) / Mathf.Max(1e-4f, SwingTravel(2) + SwingTravel(3));
                turn = Mathf.Lerp(turnBack, -turnThrough, u);
            }

            if (s > T4)
            {
                turn *= 1f - Smooth((s - T4) / Mathf.Max(0.05f, recoveryTime));
            }

            return sign * turn;
        }

        /// <summary>The hips' share of the turn, ahead of the weapon.</summary>
        public float HipTurnAt(float t) => HipShare * TurnAt(t + HipLead / SwingDuration);

        /// <summary>The chest's share of the turn (on top of the hips'), between the hips and the weapon.</summary>
        public float ChestTurnAt(float t) => (1f - HipShare) * TurnAt(t + ChestLead / SwingDuration);

        /// <summary>
        /// Swing: the angle on the arc at a progress (degrees; 0 is the contact, negative behind it).
        /// It runs back from the guard to the wound-up angle, then forward through the contact.
        /// Thrust: the distance travelled (metres, nominal) in the same way.
        /// </summary>
        public float SwingAngle(float progress)
        {
            float a = Mathf.Clamp01(progress) * SwingTotal;
            float a0 = SwingTravel(0) + SwingTravel(1);
            if (a <= a0)
            {
                return -guardAngle - a;
            }

            return -(windUpAngle + SwingCreep) + (a - a0);
        }
    }
}
