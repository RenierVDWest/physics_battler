using System;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// One moment of a captured loop, as offsets from the loop's mean pose (the stance and the guards
    /// hold the mean). Hips in leg lengths (x right, z forward) and the drop down from the loop's
    /// highest point; turns, lean, roll and the head in degrees; the hands in metres on the medium
    /// body (x right, y up, z forward, in the chest's facing).
    /// </summary>
    [Serializable]
    public struct RhythmClipKey
    {
        public Vector3 hips;
        public float drop;
        [Tooltip("The chest's turn (+ to the right); the pelvis follows part of it…")]
        public float turn;
        [Tooltip("…and turns this much more on its own.")]
        public float hipTurn;
        public float lean, roll;
        [Tooltip("Nod (+ down) and turn against the chest.")]
        public Vector2 head;
        public Vector3 main, off;
    }

    /// <summary>
    /// A Fight Rhythm's base loop taken from motion capture (a looping idle, read by
    /// Tools/Mocap/mixamo_rhythm.py) instead of the made-up bounce and sway: the hips' rock and
    /// bounce, the body's turns, the head and both hands, keyed evenly over the loop and played
    /// smoothly round it. Its mean pose belongs in the stance (combat style) and the guards.
    /// </summary>
    [Serializable]
    public class RhythmClip
    {
        [Tooltip("Where it came from.")]
        public string source;
        [Tooltip("Seconds round the loop on the medium body (slower on bigger ones).")]
        public float length = 2f;
        [Tooltip("How much of the captured motion plays (1: all of it).")]
        [Range(0f, 1.5f)] public float strength = 1f;
        public RhythmClipKey[] keys = Array.Empty<RhythmClipKey>();

        public bool IsEmpty => keys == null || keys.Length < 2 || length <= 0f;
    }

    /// <summary>The captured loops (generated files: RhythmClips.*.g.cs).</summary>
    public static partial class RhythmClips
    {
        private static RhythmClipKey K(Vector3 hips, float drop, float turn, float hipTurn, float lean, float roll, Vector2 head, Vector3 main, Vector3 off)
        {
            return new RhythmClipKey { hips = hips, drop = drop, turn = turn, hipTurn = hipTurn, lean = lean, roll = roll, head = head, main = main, off = off };
        }
    }
}
