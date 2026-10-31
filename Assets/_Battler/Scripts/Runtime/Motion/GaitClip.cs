using System;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// A walk cycle from motion capture (Tools/Mocap/mixamo_rhythm.py --gait): when the unit walks,
    /// its feet step to the capture's cadence, lift and width, and its body moves as the capture's
    /// does (the hips' bob and sway against an evenly moving root, the turns, the lean, the head and
    /// both hands swinging round their guard). Lengths in leg lengths, angles in degrees, the hands in
    /// metres on the medium body; phases 0 to 1 round the cycle (two steps).
    /// </summary>
    [Serializable]
    public class GaitClip
    {
        [Tooltip("Where it came from.")]
        public string source;
        [Tooltip("Seconds for a cycle (two steps) on the medium body at the capture's own speed.")]
        public float length = 1f;
        [Tooltip("How far a cycle travels, in leg lengths.")]
        public float distance = 1.5f;
        [Tooltip("The feet apart side to side, in leg lengths.")]
        public float width = 0.2f;
        [Tooltip("How high a foot lifts, in leg lengths.")]
        public float stepHeight = 0.15f;
        [Tooltip("The walk's mean spine lean, how low its hips are at their highest (leg lengths below straight), and its pelvis and chest turns: what the walk changes from the stance.")]
        public float lean;
        public float crouch;
        public float pelvisTurn;
        public float chestTurn;
        [Tooltip("When each foot lifts and lands, round the cycle.")]
        [Range(0f, 1f)] public float liftL, landL, liftR, landR;
        [Tooltip("How much of the captured body motion plays (1: all of it).")]
        [Range(0f, 1.5f)] public float strength = 1f;
        [Tooltip("The body's motion round the cycle.")]
        public RhythmClip body = new RhythmClip();

        public bool IsEmpty => body == null || body.IsEmpty || length <= 0f || distance <= 0f;

        public float Lift(int foot) => foot == Stance.Left ? liftL : liftR;
        public float Land(int foot) => foot == Stance.Left ? landL : landR;

        /// <summary>The share of the cycle a foot is in the air (as a walk: 35 to 42%, so one foot is always down).</summary>
        public float Swing(int foot) => Mathf.Clamp(Mathf.Repeat(Land(foot) - Lift(foot), 1f), 0.35f, 0.42f);
    }
}
