using System;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// The unit's Fight Rhythm (<see cref="RhythmPlayer"/>): how it moves between blows. Each step
    /// on its feet with nothing else to do, the rhythm's base loop and posture go onto the pose; a
    /// posture's planned steps go to the stance, its travel moves the unit, and a weapon feint plays
    /// the unit's own blow's wind-up part of the way and back (no hits). Blows, blocks and being off
    /// balance pause it. The fight logic asks for a lead-in before a blow (<see cref="LeadIn"/>).
    /// </summary>
    public partial class UnitController
    {
        /// <summary>A unit started a posture (true: as a lead-in to a blow).</summary>
        public static event Action<UnitController, PostureDefinition, bool> PostureStarted;

        /// <summary>A unit's posture ended (true: cut short).</summary>
        public static event Action<UnitController, PostureDefinition, bool> PostureEnded;

        public RhythmPlayer Rhythm { get; private set; }

        /// <summary>Who it is squaring up to (set by the fight logic): feints aim at it, circling goes round it.</summary>
        public UnitController Opponent { get; set; }

        /// <summary>The body parts open to a blow right now (a posture's window, a tired guard).</summary>
        public AimParts Openings => Rhythm != null && IsAlive && State == UnitState.Standing ? Rhythm.Open : AimParts.None;

        private MoveState feint;
        private bool postureHolds;

        /// <summary>Off, the unit stands in its plain guard (tests that measure blows and hits exactly).</summary>
        public bool RhythmOn { get; set; } = true;

        /// <summary>The rhythm's base loop only (no posture), for the standing pose a get-up blends into.</summary>
        private void PoseRhythmBase()
        {
            Pose.Rhythm = Rhythm != null && RhythmOn ? Rhythm.Sample(Body.LegLength, Body.SizeFactor) : default;
            Pose.Feint = null;
        }

        private void InitializeRhythm(int seed)
        {
            var definition = Style.rhythm != null ? Style.rhythm : RhythmLibrary.For(profile.body);
            Rhythm = new RhythmPlayer(definition, seed + 31);
            Rhythm.SetSize(Body.SizeFactor);
            Rhythm.Started += OnPostureStarted;
            Rhythm.Ended += OnPostureEnded;
        }

        /// <summary>
        /// Before a blow: maybe a lead-in posture first (a feint), as the style likes. Returns how many
        /// seconds it takes (the blow waits for it), or 0 to strike now.
        /// </summary>
        public float LeadIn()
        {
            if (Rhythm == null || !CanAct || CurrentMove != null)
            {
                return 0f;
            }

            Rhythm.HasOpponent = Opponent != null && Opponent.IsAlive;
            return Rhythm.TryLeadIn();
        }

        /// <summary>The rhythm's step: called from the unit's step, on its feet, before the pose.</summary>
        private void UpdateRhythm(float dt)
        {
            if (Rhythm == null)
            {
                return;
            }

            // Free to posture: on its feet, no blow (nor one fading out), not blocking, not frozen.
            bool free = State == UnitState.Standing && CurrentMove == null && cutShort == null && BlockBlend < 0.05f && HitStop <= 0f;
            Rhythm.HasOpponent = Opponent != null && Opponent.IsAlive && !Opponent.IsDown;
            Rhythm.Update(dt, free && RhythmOn);

            if (free)
            {
                foreach (var step in Rhythm.DueSteps)
                {
                    TakePostureStep(step);
                }

                if (Rhythm.TravelDelta.sqrMagnitude > 0f)
                {
                    transform.position += transform.rotation * (Rhythm.TravelDelta * Body.LegLength);
                }

                if (Mathf.Abs(Rhythm.Circle) > 1e-4f && Opponent != null)
                {
                    transform.position += transform.right * (Rhythm.Circle * Body.SizeFactor * dt);
                }
            }

            UpdateFeint();

            // (A real blow takes over from the rhythm as it blends in: the pose pipeline weighs it.)
            Pose.Rhythm = RhythmOn ? Rhythm.Sample(Body.LegLength, Body.SizeFactor) : default;
            Pose.Feint = feint;
        }

        private void TakePostureStep(PostureStep step)
        {
            int foot = step.foot == PostureFoot.Lead ? Stance.Lead : 1 - Stance.Lead;
            Vector3 home = Stance.HomeWorld(foot, transform.position, facingYaw);
            Vector3 to = step.to == PostureStepTo.Home ? home : home + transform.rotation * (step.offset * Body.LegLength);
            float seconds = Rhythm.Current != null ? Mathf.Max(0.08f, step.length * Rhythm.Length(Rhythm.Current)) : 0.2f;
            Stance.StepTo(foot, to, facingYaw + Stance.HomeYawLocal(foot), seconds);
        }

        /// <summary>A weapon feint: the blow's own wind-up, part of the way and back, posed but never swung.</summary>
        private void UpdateFeint()
        {
            if (feint == null)
            {
                return;
            }

            if (Pose.Move != feint || CurrentMove != null || State != UnitState.Standing)
            {
                feint = null;
                return;
            }

            float depth = Rhythm.Current != null ? Rhythm.Current.feintDepth : 0f;
            feint.Time = Rhythm.Feint * depth * feint.Move.WindUpEnd * feint.Duration;
            if (Rhythm.Current == null)
            {
                Pose.Move = null;
                feint = null;
            }
        }

        private void OnPostureStarted(PostureDefinition posture, bool leadIn)
        {
            postureHolds = posture.steps.Length > 0 && CurrentMove == null;
            if (postureHolds)
            {
                Stance.Hold = true;   // only the posture's own steps
            }

            if (!string.IsNullOrEmpty(posture.feintMove) && Opponent != null && Loadout?.Main != null && CurrentMove == null && Pose.Move == null)
            {
                MoveDefinition move = null;
                foreach (var m in Loadout.Main.moves)
                {
                    if (m != null && (m.name == "MD_" + posture.feintMove || m.name == posture.feintMove))
                    {
                        move = m;
                    }
                }

                if (move != null)
                {
                    var part = move.targets.Contains(AimPart.Chest) ? AimPart.Chest : move.targets.Contains(AimPart.Head) ? AimPart.Head : AimPart.Belly;
                    var state = MovePlanner.Prepare(this, Opponent, move, part);
                    if (state != null)
                    {
                        state.Time = 0f;
                        feint = state;
                        Pose.Move = state;
                    }
                }
            }

            PostureStarted?.Invoke(this, posture, leadIn);
        }

        private void OnPostureEnded(PostureDefinition posture, bool interrupted)
        {
            if (postureHolds && CurrentMove == null)
            {
                Stance.Hold = false;
            }

            postureHolds = false;
            if (feint != null && Pose.Move == feint)
            {
                Pose.Move = null;
            }

            feint = null;
            PostureEnded?.Invoke(this, posture, interrupted);
        }

        /// <summary>Stops the rhythm's posture at once (knocked down, moved by the sandbox).</summary>
        private void StopRhythm()
        {
            Rhythm?.Stop();
            if (feint != null && Pose.Move == feint)
            {
                Pose.Move = null;
            }

            feint = null;
            Pose.Rhythm = default;
            Pose.Feint = null;
        }
    }
}
