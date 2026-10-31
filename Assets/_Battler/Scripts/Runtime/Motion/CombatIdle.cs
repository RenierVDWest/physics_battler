using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>What the idle movement adds to the stance at one moment.</summary>
    public struct IdleSample
    {
        /// <summary>Hips drift, in the unit's space (metres; x right, z forward).</summary>
        public Vector3 hipShift;
        /// <summary>Up and down, metres.</summary>
        public float bob;
        /// <summary>Hips: pitch, yaw, roll in degrees.</summary>
        public Vector3 hips;
        /// <summary>Chest: pitch, yaw, roll in degrees.</summary>
        public Vector3 chest;
        /// <summary>Head: pitch, yaw, roll in degrees.</summary>
        public Vector3 head;
    }

    /// <summary>
    /// The constant movement of a fighter between blows: a slow loop of the hips, a bob, the chest
    /// and head weaving. Smooth, never repeating exactly (sums of sines at unrelated frequencies),
    /// and different for every unit (its own phases). Amplitudes come from the combat style, in
    /// proportion to the leg length.
    /// </summary>
    public sealed class CombatIdle
    {
        private readonly float[] phase = new float[12];

        public CombatIdle(int seed)
        {
            var random = new System.Random(seed);
            for (int i = 0; i < phase.Length; i++)
            {
                phase[i] = (float)(random.NextDouble() * Mathf.PI * 2.0);
            }
        }

        /// <summary>Smooth noise in [-1, 1].</summary>
        private float Wave(float t, float frequency, int channel)
        {
            float w = 2f * Mathf.PI * frequency * t;
            return 0.62f * Mathf.Sin(w + phase[channel]) + 0.38f * Mathf.Sin(1.73f * w + phase[(channel + 5) % phase.Length]);
        }

        public IdleSample Sample(float time, CombatStyle style, float legLength)
        {
            var s = new IdleSample();
            float sway = style.swayAmplitude * legLength;
            s.hipShift = new Vector3(sway * Wave(time, style.swayFrequency, 0),
                                     0f,
                                     0.6f * sway * Wave(time, style.swayFrequency * 0.83f, 1));
            // Bob: a bounce, mostly down from the stance height (|sin|-like).
            float bounce = Mathf.Abs(Mathf.Sin(Mathf.PI * style.bobFrequency * time + phase[2]));
            s.bob = -style.bobAmplitude * legLength * bounce;
            float weave = style.weaveDegrees;
            float f = style.weaveFrequency;
            s.hips = new Vector3(0.3f * weave * Wave(time, f * 0.9f, 3), 0.8f * weave * Wave(time, f * 0.7f, 4),
                                 0.5f * weave * Wave(time, style.swayFrequency, 0));
            s.chest = new Vector3(0.6f * weave * Wave(time, f, 6), weave * Wave(time, f * 1.1f, 7),
                                  0.7f * weave * Wave(time, f * 0.8f, 8));
            s.head = new Vector3(0.4f * weave * Wave(time, f * 1.3f, 9), 0.6f * weave * Wave(time, f * 0.6f, 10),
                                 0.3f * weave * Wave(time, f * 1.2f, 11));
            return s;
        }
    }
}
