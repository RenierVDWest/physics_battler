using System;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// The unit's tempo (milestone 8, <see cref="TempoDirector"/>): whether it is gathering, probing,
    /// pressing, shelled up or countering. Each step it feeds the director what it knows (its energy
    /// and aggression, whether it is in a blow or walking; the fight loop says whether it is engaged
    /// and whether its target is worth pressing), gives the rhythm the energy the tempo earns, picks
    /// the postures the state allows, and holds the state's stance on the pose (blended over 0.3 s).
    /// It pays for its blows and counts the blows coming at it as pressure.
    /// </summary>
    public partial class UnitController
    {
        /// <summary>A unit's tempo changed: (unit, from, to, why).</summary>
        public static event Action<UnitController, TempoState, TempoState, string> TempoChanged;

        public TempoDirector Tempo { get; private set; }

        /// <summary>Close to the one it is fighting (set by the fight loop).</summary>
        public bool TempoEngaged { get; set; }

        /// <summary>The one it is fighting is reeling or nearly done (set by the fight loop).</summary>
        public bool TargetVulnerable { get; set; }

        /// <summary>Off, the unit has no tempo (energy as before the tempo; tests that measure the rhythm alone).</summary>
        public bool TempoOn { get; set; } = true;

        // The stance held: blended from where it was to the state's, eased in and out (no jolt).
        private MoodStance mood = MoodStance.Neutral, moodFrom = MoodStance.Neutral, moodTo = MoodStance.Neutral;
        private float moodBlend = 1f;
        private TempoState moodState;
        private bool moodStanding;

        private void InitializeTempo(int seed)
        {
            var definition = Rhythm != null ? Rhythm.Definition : null;
            Tempo = new TempoDirector(definition != null ? definition.temperament : new Temperament(), seed + 53);
            Tempo.Changed += (from, to, why) => TempoChanged?.Invoke(this, from, to, why);
            mood = moodFrom = moodTo = MoodStance.Neutral;
            moodState = Tempo.State;
        }

        /// <summary>The tempo's step: before the rhythm's, so its posture filter and loop pace apply this step.</summary>
        private void UpdateTempo(float dt)
        {
            if (Tempo == null || Rhythm == null)
            {
                return;
            }

            bool on = TempoOn && RhythmOn;
            Rhythm.TempoDriven = on;
            if (!on)
            {
                Rhythm.Use = TempoUse.All;
                Rhythm.LoopSpeed = Rhythm.LoopSize = 1f;
                Pose.Mood = default;
                return;
            }

            bool walking = Gait != null && Gait.Weight >= 0.3f;
            Tempo.Update(dt, new TempoInput
            {
                energy = Rhythm.Energy,
                aggression = Rhythm.Aggression,
                canAct = CanAct && IsAlive,
                attacking = CurrentMove != null,
                walking = walking,
                engaged = TempoEngaged,
                targetVulnerable = TargetVulnerable,
            });
            Rhythm.Recharge(Tempo.Regen * dt);
            Rhythm.Use = Tempo.Use;
            Rhythm.Walking = walking;

            // The state's stance, blended in over about 0.3 s (none off its feet).
            bool standing = CanAct && IsAlive && State == UnitState.Standing;
            if (standing != moodStanding || Tempo.State != moodState)
            {
                moodStanding = standing;
                moodState = Tempo.State;
                moodFrom = mood;
                moodTo = standing ? Tempo.Temperament.StanceFor(Tempo.State) : MoodStance.Neutral;
                if (moodTo.loopSpeed <= 0f) moodTo.loopSpeed = 1f;
                if (moodTo.loopSize <= 0f) moodTo.loopSize = 1f;
                moodBlend = 0f;
            }

            moodBlend = Mathf.Min(1f, moodBlend + dt / MoodBlendTime);
            mood = MoodStance.Lerp(moodFrom, moodTo, Mathf.SmoothStep(0f, 1f, moodBlend));
            Rhythm.LoopSpeed = mood.loopSpeed;
            Rhythm.LoopSize = mood.loopSize;
            float leg = Body.LegLength, size = Body.SizeFactor;
            Pose.Mood = new RhythmSample
            {
                hips = new Vector3(0f, 0f, mood.forward * leg),
                drop = mood.crouch * leg,
                lean = mood.lean,
                head = new Vector2(mood.chin, 0f),
                main = mood.main * size,
                off = mood.off * size,
            };
        }

        /// <summary>Seconds a change of stance takes (eased in and out).</summary>
        public const float MoodBlendTime = 0.4f;

        /// <summary>Off its feet: the stance lets go (it comes back in, blended, once it stands).</summary>
        private void ResetMood()
        {
            mood = moodFrom = moodTo = MoodStance.Neutral;
            moodBlend = 1f;
            moodStanding = false;
            Pose.Mood = default;
        }

        /// <summary>Its blow starts: the tempo counts it and it pays for it.</summary>
        private void TempoBlowStarted()
        {
            if (Tempo == null || !TempoOn || Rhythm == null)
            {
                return;
            }

            Rhythm.SpendEnergy(Tempo.BlowCost);
            Tempo.NoteBlowStarted();
        }
    }
}
