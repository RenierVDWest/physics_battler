using System;

namespace UnityEngine
{
    // Unity's curve, evaluated the same way: cubic Hermite between keys (unweighted tangents),
    // clamped before the first and after the last key.
    public struct Keyframe
    {
        public float time;
        public float value;
        public float inTangent;
        public float outTangent;

        public Keyframe(float time, float value) : this(time, value, 0f, 0f) { }

        public Keyframe(float time, float value, float inTangent, float outTangent)
        {
            this.time = time;
            this.value = value;
            this.inTangent = inTangent;
            this.outTangent = outTangent;
        }
    }

    [Serializable]
    public class AnimationCurve
    {
        private Keyframe[] keyArray;

        public AnimationCurve(params Keyframe[] keys) { keyArray = keys ?? new Keyframe[0]; }

        public Keyframe[] keys { get => (Keyframe[])keyArray.Clone(); set => keyArray = value ?? new Keyframe[0]; }

        public int length => keyArray.Length;

        public Keyframe this[int index] => keyArray[index];

        public static AnimationCurve Linear(float t0, float v0, float t1, float v1)
        {
            float s = (v1 - v0) / (t1 - t0);
            return new AnimationCurve(new Keyframe(t0, v0, 0f, s), new Keyframe(t1, v1, s, 0f));
        }

        public static AnimationCurve EaseInOut(float t0, float v0, float t1, float v1)
        {
            return new AnimationCurve(new Keyframe(t0, v0, 0f, 0f), new Keyframe(t1, v1, 0f, 0f));
        }

        public float Evaluate(float t)
        {
            if (keyArray.Length == 0) return 0f;
            if (t <= keyArray[0].time) return keyArray[0].value;
            var last = keyArray[keyArray.Length - 1];
            if (t >= last.time) return last.value;
            for (int i = 0; i < keyArray.Length - 1; i++)
            {
                var a = keyArray[i];
                var b = keyArray[i + 1];
                if (t > b.time) continue;
                float dt = b.time - a.time;
                if (dt <= 0f) return b.value;
                float s = (t - a.time) / dt;
                float s2 = s * s, s3 = s2 * s;
                float h00 = 2f * s3 - 3f * s2 + 1f, h10 = s3 - 2f * s2 + s, h01 = -2f * s3 + 3f * s2, h11 = s3 - s2;
                return h00 * a.value + h10 * dt * a.outTangent + h01 * b.value + h11 * dt * b.inTangent;
            }

            return last.value;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ExecuteAlways : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SelectionBaseAttribute : Attribute { }
}
