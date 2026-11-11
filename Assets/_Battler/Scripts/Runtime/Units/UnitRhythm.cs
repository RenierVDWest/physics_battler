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

        /// <summary>A posture already under way became the lead-in to a blow.</summary>
        public static event Action<UnitController, PostureDefinition> PostureLedIn;

        public RhythmPlayer Rhythm { get; private set; }

        /// <summary>Who it is squaring up to (set by the fight logic): feints aim at it, circling goes round it.</summary>
        public UnitController Opponent { get; set; }

        /// <summary>The body parts open to a blow right now (a posture's window, a tired guard).</summary>
        public AimParts Openings => Rhythm != null && IsAlive && State == UnitState.Standing ? Rhythm.Open : AimParts.None;

        /// <summary>It may circle its opponent (set by the fight logic: not while just facing each other).</summary>
        public bool CircleOpponent { get; set; } = true;

        /// <summary>How fast it is circling right now, m/s to its right (0: not circling).</summary>
        public float Circling { get; private set; }

        private MoveState feint;
        private bool postureHolds;

        // The order units were made in (who leads when two circle each other).
        private static int rhythmCount;
        private int rhythmOrder;

        // Where the root stood after the last rhythm step, and how far something else (walking in,
        // a push) has moved it since the posture holding the feet began.
        private Vector3 rhythmRoot;
        private float heldDrift;

        /// <summary>Off, the unit stands in its plain guard (tests that measure blows and hits exactly).</summary>
        public bool RhythmOn { get; set; } = true;

        private void InitializeRhythm(int seed)
        {
            var definition = Style.rhythm != null ? Style.rhythm : RhythmLibrary.For(profile.body);
            Rhythm = new RhythmPlayer(definition, seed + 31);
            Rhythm.SetSize(Body.SizeFactor);
            Rhythm.Started += OnPostureStarted;
            Rhythm.Ended += OnPostureEnded;
            Rhythm.LedIn += p => PostureLedIn?.Invoke(this, p);
            rhythmRoot = transform.position;
            rhythmOrder = ++rhythmCount;
        }

        /// <summary>
        /// Before a blow: maybe a lead-in posture first (a feint), as the style likes. Returns how many
        /// seconds it takes (the blow waits for it), or 0 to strike now.
        /// </summary>
        public float LeadIn()
        {
            if (Rhythm == null || !RhythmOn || !CanAct || !FreeToPosture() || (Gait != null && Gait.Weight >= 0.3f))
            {
                return 0f;
            }

            Rhythm.HasOpponent = Opponent != null && Opponent.IsAlive;
            return Rhythm.TryLeadIn();
        }

        private void OnPostureStarted(PostureDefinition posture, bool leadIn)
        {
            // (A flourish holds the feet too: a shuffle step under a big twirl of the stick throws the arm.)
            postureHolds = (posture.steps.Length > 0 || (posture.kind == PostureKind.Flourish && (Gait == null || Gait.Weight < 0.3f))) && CurrentMove == null;
            heldDrift = 0f;
            rhythmRoot = transform.position;
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
            ReleasePostureHold();
            if (feint != null && Pose.Move == feint)
            {
                Pose.Move = null;
            }

            feint = null;
            PostureEnded?.Invoke(this, posture, interrupted);
        }

        /// <summary>On its feet, no blow (nor one fading out), not blocking, not frozen.</summary>
        private bool FreeToPosture()
        {
            return IsAlive && State == UnitState.Standing && CurrentMove == null && cutShort == null && BlockBlend < 0.05f && HitStop <= 0f;
        }

        /// <summary>The posture lets go of the feet (a blow under way keeps its own hold).</summary>
        private void ReleasePostureHold()
        {
            if (postureHolds && CurrentMove == null)
            {
                Stance.Hold = gaitHolds;   // (a walk keeps its own hold)
            }

            postureHolds = false;
        }

        /// <summary>Stops the rhythm's posture at once (knocked down, moved by the sandbox, killed).</summary>
        private void StopRhythm()
        {
            Rhythm?.Stop();
            if (feint != null && Pose.Move == feint)
            {
                Pose.Move = null;
            }

            feint = null;
            Pose.Rhythm = default;
            ResetMood();
            Pose.Feint = null;
            Circling = 0f;
            rhythmRoot = transform.position;
        }
    }
}
