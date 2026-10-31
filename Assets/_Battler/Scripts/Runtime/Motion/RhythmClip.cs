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

        /// <summary>The loop at <paramref name="phase"/> (0 to 1, wrapping), smoothed between keys.</summary>
        public RhythmClipKey Sample(float phase)
        {
            int n = keys.Length;
            float f = Mathf.Repeat(phase, 1f) * n;
            int i1 = Mathf.FloorToInt(f) % n;
            float u = f - Mathf.Floor(f);
            var k0 = keys[(i1 + n - 1) % n];
            var k1 = keys[i1];
            var k2 = keys[(i1 + 1) % n];
            var k3 = keys[(i1 + 2) % n];
            return new RhythmClipKey
            {
                hips = CatmullRom(k0.hips, k1.hips, k2.hips, k3.hips, u),
                drop = CatmullRom(k0.drop, k1.drop, k2.drop, k3.drop, u),
                turn = CatmullRom(k0.turn, k1.turn, k2.turn, k3.turn, u),
                hipTurn = CatmullRom(k0.hipTurn, k1.hipTurn, k2.hipTurn, k3.hipTurn, u),
                lean = CatmullRom(k0.lean, k1.lean, k2.lean, k3.lean, u),
                roll = CatmullRom(k0.roll, k1.roll, k2.roll, k3.roll, u),
                head = CatmullRom(k0.head, k1.head, k2.head, k3.head, u),
                main = CatmullRom(k0.main, k1.main, k2.main, k3.main, u),
                off = CatmullRom(k0.off, k1.off, k2.off, k3.off, u),
            };
        }

        private static float CatmullRom(float a, float b, float c, float d, float u)
        {
            return 0.5f * (2f * b + (c - a) * u + (2f * a - 5f * b + 4f * c - d) * u * u + (3f * b - a - 3f * c + d) * u * u * u);
        }

        private static Vector2 CatmullRom(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float u)
        {
            return new Vector2(CatmullRom(a.x, b.x, c.x, d.x, u), CatmullRom(a.y, b.y, c.y, d.y, u));
        }

        private static Vector3 CatmullRom(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float u)
        {
            return new Vector3(CatmullRom(a.x, b.x, c.x, d.x, u), CatmullRom(a.y, b.y, c.y, d.y, u), CatmullRom(a.z, b.z, c.z, d.z, u));
        }
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
