using System;
using UnityEngine;

namespace PhysicsBattler.Units
{
    public enum UnitStateKind
    {
        Steady,
        Flinch,
        Stagger,
        Knockdown,
        GettingUp,
        Dead,
    }

    /// <summary>What the body is doing this step, measured by the controller.</summary>
    public struct BodySense
    {
        public float tiltDegrees;      // chest up vs world up
        public float pelvisHeight;     // above the ground
        public float restPelvisHeight;
        public float pelvisSpeed;
        public bool grounded;
    }

    /// <summary>
    /// Stability ("poise") and the state it puts the unit in. Hits and shoves take stability away;
    /// it grows back after a short delay. The state sets how hard the balance assist holds the
    /// unit up and how strong its muscles are. A body tipped past the point of no return falls
    /// whatever its stability says, so the state stays honest to what the body is doing.
    /// </summary>
    public sealed class UnitState
    {
        private readonly UnitProfile profile;
        private float timeInState;
        private float sinceLoss;
        private float restTimer;

        public UnitStateKind Kind { get; private set; } = UnitStateKind.Steady;
        public float Stability { get; private set; }
        public float Health { get; private set; }
        public bool IsAlive => Kind != UnitStateKind.Dead;
        public bool IsStanding => Kind == UnitStateKind.Steady || Kind == UnitStateKind.Flinch;
        public float TimeInState => timeInState;

        /// <summary>Why the unit entered its current state (for the playtest log).</summary>
        public string LastReason { get; private set; } = "spawned";

        /// <summary>0..1 through getting up.</summary>
        public float GetUpProgress => Kind == UnitStateKind.GettingUp
            ? Mathf.Clamp01(timeInState / Mathf.Max(0.1f, profile.getUpDuration))
            : 1f;

        public event Action<UnitStateKind, UnitStateKind> Changed;

        public UnitState(UnitProfile profile)
        {
            this.profile = profile;
            Stability = profile.maxStability;
            Health = profile.maxHealth;
        }

        /// <summary>How strongly the balance assist holds the unit up (0..1).</summary>
        public float Balance
        {
            get
            {
                switch (Kind)
                {
                    case UnitStateKind.Steady: return 1f;
                    case UnitStateKind.Flinch: return 0.85f;
                    case UnitStateKind.Stagger:
                        return Mathf.Lerp(0.3f, 1f, Mathf.InverseLerp(profile.knockdownBelow, profile.staggerBelow, Stability));
                    case UnitStateKind.GettingUp: return Mathf.SmoothStep(0f, 1f, GetUpProgress);
                    default: return 0f;
                }
            }
        }

        /// <summary>Overall muscle strength (0..1); hits add local dips on top.</summary>
        public float MuscleTone
        {
            get
            {
                switch (Kind)
                {
                    case UnitStateKind.Steady:
                    case UnitStateKind.Flinch: return 1f;
                    case UnitStateKind.Stagger: return 0.75f;
                    case UnitStateKind.Knockdown: return profile.knockdownMuscle;
                    case UnitStateKind.GettingUp: return Mathf.Lerp(profile.knockdownMuscle, 1f, GetUpProgress);
                    default: return Mathf.Lerp(0.05f, 0f, Mathf.Clamp01(timeInState / 0.3f));
                }
            }
        }

        public void LoseStability(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            float loss = amount / Mathf.Max(0.05f, profile.poiseResistance);
            Stability = Mathf.Max(0f, Stability - loss);
            sinceLoss = 0f;

            if (Kind == UnitStateKind.Knockdown)
            {
                return;
            }

            if (Stability < profile.knockdownBelow)
            {
                Enter(UnitStateKind.Knockdown, $"stability {Stability:0} < knockdownBelow {profile.knockdownBelow:0} (lost {loss:0.0})");
            }
            else if (Stability < profile.staggerBelow)
            {
                if (Kind != UnitStateKind.Stagger)
                {
                    Enter(UnitStateKind.Stagger, $"stability {Stability:0} < staggerBelow {profile.staggerBelow:0} (lost {loss:0.0})");
                }
            }
            else if (loss >= profile.flinchThreshold && Kind == UnitStateKind.Steady)
            {
                Enter(UnitStateKind.Flinch, $"lost {loss:0.0} stability (flinch threshold {profile.flinchThreshold:0.0})");
            }
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            Health = Mathf.Max(0f, Health - amount);
            if (Health <= 0f)
            {
                Enter(UnitStateKind.Dead, $"health 0 (took {amount:0.0})");
            }
        }

        public void Kill()
        {
            Health = 0f;
            if (IsAlive)
            {
                Enter(UnitStateKind.Dead, "killed");
            }
        }

        /// <summary>The body fell whatever its stability says (tipped over, or out of catch steps).</summary>
        public void ForceKnockdown(string reason)
        {
            if (IsAlive && Kind != UnitStateKind.Knockdown)
            {
                Stability = Mathf.Min(Stability, profile.knockdownBelow);
                Enter(UnitStateKind.Knockdown, reason);
            }
        }

        public void Step(float dt, in BodySense sense, bool caughtBalance)
        {
            timeInState += dt;
            sinceLoss += dt;
            if (!IsAlive)
            {
                return;
            }

            if (sinceLoss > profile.regenDelay && Kind != UnitStateKind.Knockdown)
            {
                Stability = Mathf.Min(profile.maxStability, Stability + profile.regenRate * dt);
            }

            bool tiltedOver = sense.tiltDegrees > profile.maxTiltDegrees;
            bool sunk = sense.pelvisHeight < sense.restPelvisHeight * 0.55f;
            bool tipped = tiltedOver || sunk;

            switch (Kind)
            {
                case UnitStateKind.Steady:
                case UnitStateKind.Flinch:
                case UnitStateKind.Stagger:
                    if (tipped)
                    {
                        ForceKnockdown(tiltedOver
                            ? $"tipped: chest tilt {sense.tiltDegrees:0}° > maxTiltDegrees {profile.maxTiltDegrees:0}°"
                            : $"tipped: pelvis {sense.pelvisHeight:0.00} m < 55% of rest {sense.restPelvisHeight:0.00} m");
                    }
                    else if (Kind == UnitStateKind.Flinch && timeInState > 0.3f)
                    {
                        Enter(UnitStateKind.Steady, "flinch over");
                    }
                    else if (Kind == UnitStateKind.Stagger && timeInState > 0.6f
                             && Stability >= profile.staggerBelow && caughtBalance)
                    {
                        Enter(UnitStateKind.Steady, $"caught its balance (stability {Stability:0})");
                    }

                    break;

                case UnitStateKind.Knockdown:
                    restTimer = sense.pelvisSpeed < profile.restSpeed ? restTimer + dt : 0f;
                    if (timeInState > profile.knockdownMinTime && restTimer > profile.restTime)
                    {
                        Stability = Mathf.Max(Stability, profile.maxStability * 0.7f);
                        Enter(UnitStateKind.GettingUp, $"down {timeInState:0.0} s, at rest {restTimer:0.0} s");
                    }

                    break;

                case UnitStateKind.GettingUp:
                    if (timeInState >= profile.getUpDuration)
                    {
                        Enter(UnitStateKind.Steady, $"get-up time over ({profile.getUpDuration:0.0} s)");
                    }

                    break;
            }
        }

        private void Enter(UnitStateKind next, string reason)
        {
            var previous = Kind;
            LastReason = reason;
            Kind = next;
            timeInState = 0f;
            restTimer = 0f;
            Changed?.Invoke(previous, next);
        }
    }
}
