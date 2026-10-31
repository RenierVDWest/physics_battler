using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// What a unit is swinging, seen from its shoulder: the arm and the weapon (with the hand), swung
    /// out at the swing's reach. Measured from the real rigidbodies, so a heavier or longer weapon is
    /// slower to move and to stop, and needs more strength to swing at all.
    /// </summary>
    public struct SwingLoad
    {
        /// <summary>Moment of inertia about the shoulder (kg·m²).</summary>
        public float inertia;

        /// <summary>Σ mass × distance from the shoulder (kg·m): gravity's torque is g times this, arm out flat.</summary>
        public float massMoment;

        /// <summary>The weapon's own mass (kg) and its centre of mass from the grip (m), with the hand.</summary>
        public float weaponMass;
        public float weaponCentre;

        /// <summary>Torque (N·m) it takes to hold the weapon out flat.</summary>
        public float HoldTorque => massMoment * Mathf.Abs(Physics.gravity.y);
    }

    /// <summary>
    /// The swing as a heavy thing moved by limited strength. The ghost's swing angle is not keyed by
    /// time: it is integrated from the torque the wielder can put in (a share of its strength per
    /// phase) and gravity, against the load's inertia. Paths between poses (the wind-up from the carry,
    /// the recovery) use the time-optimal accelerate-then-brake profile for the same strength.
    /// </summary>
    public static class SwingDynamics
    {
        /// <summary>Gravity's torque (N·m) on the swing at angle θ: positive turns it toward +θ.</summary>
        public static float GravityTorque(in SwingPlan plan, float theta, float massMoment)
        {
            Vector3 direction = SwingSolver.Direction(plan, theta);
            Vector3 tangent = Vector3.Cross(plan.axis, direction);   // d(direction)/dθ
            return massMoment * Vector3.Dot(Physics.gravity, tangent);
        }

        /// <summary>
        /// Angular acceleration (rad/s²) a unit can give the load over a whole move, after holding up its
        /// weight: (effort × strength − half the hold torque) / inertia, never below a crawl.
        /// </summary>
        public static float MoveAcceleration(float strength, float effort, in SwingLoad load)
        {
            float net = strength * effort - 0.5f * load.HoldTorque;
            return Mathf.Max(0.4f, net / Mathf.Max(0.05f, load.inertia));
        }

        /// <summary>Time (s) to turn through <paramref name="angle"/> radians from rest to rest at <paramref name="acceleration"/>.</summary>
        public static float RestToRestTime(float angle, float acceleration)
        {
            return 2f * Mathf.Sqrt(Mathf.Max(0f, angle) / Mathf.Max(0.01f, acceleration));
        }

        /// <summary>Progress 0..1 along a rest-to-rest move of duration <paramref name="total"/> after <paramref name="time"/> s.</summary>
        public static float RestToRest(float time, float total)
        {
            if (total <= 1e-4f)
            {
                return 1f;
            }

            float u = Mathf.Clamp01(time / total);
            return u < 0.5f ? 2f * u * u : 1f - 2f * (1f - u) * (1f - u);
        }

        /// <summary>
        /// The fastest the strike may go and still be braked to a stop within <paramref name="followRadians"/>
        /// by <paramref name="brakeAcceleration"/>.
        /// </summary>
        public static float StoppableSpeed(float brakeAcceleration, float followRadians)
        {
            return Mathf.Sqrt(2f * Mathf.Max(0f, brakeAcceleration) * Mathf.Max(0f, followRadians));
        }

        /// <summary>
        /// One step of the strike: accelerate toward +θ with the drive plus gravity, up to the stoppable
        /// speed. θ in degrees, ω in rad/s.
        /// </summary>
        public static void StepStrike(ref float theta, ref float omega, float driveTorque, float maxOmega,
            in SwingPlan plan, in SwingLoad load, float dt)
        {
            float torque = driveTorque + GravityTorque(plan, theta, load.massMoment);
            omega = Mathf.Min(maxOmega, omega + torque / Mathf.Max(0.05f, load.inertia) * dt);
            omega = Mathf.Max(0f, omega);
            theta += omega * Mathf.Rad2Deg * dt;
        }

        /// <summary>One step of braking (follow-through): returns true once stopped.</summary>
        public static bool StepBrake(ref float theta, ref float omega, float brakeTorque, in SwingPlan plan, in SwingLoad load, float dt)
        {
            if (omega <= 0f)
            {
                omega = 0f;
                return true;
            }

            float torque = -brakeTorque + GravityTorque(plan, theta, load.massMoment);
            omega += torque / Mathf.Max(0.05f, load.inertia) * dt;
            if (omega <= 0f)
            {
                omega = 0f;
                return true;
            }

            theta += omega * Mathf.Rad2Deg * dt;
            return false;
        }

        /// <summary>
        /// Runs a strike and follow-through offline, for the log and for timing predictions: how long
        /// from the wound-up position to contact, how fast at contact (rad/s), and where it stops.
        /// </summary>
        public static void Predict(in SwingPlan plan, in SwingLoad load, float strength, AttackDefinition d,
            out float timeToContact, out float contactOmega, out float stopAngle)
        {
            float drive = strength * d.strikeEffort;
            float brake = strength * d.brakeEffort;
            float maxOmega = StoppableSpeed(brake / Mathf.Max(0.05f, load.inertia), d.followAngle * Mathf.Deg2Rad);
            float theta = d.windUpAngle;
            float omega = 0f;
            const float dt = 1f / 120f;
            timeToContact = 0f;
            for (int i = 0; i < 1200 && theta < 0f; i++)
            {
                StepStrike(ref theta, ref omega, drive, maxOmega, plan, load, dt);
                timeToContact += dt;
            }

            contactOmega = omega;
            for (int i = 0; i < 1200; i++)
            {
                if (StepBrake(ref theta, ref omega, brake, plan, load, dt))
                {
                    break;
                }
            }

            stopAngle = theta;
        }
    }
}
