using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>Right-drag to orbit, middle-drag to pan, scroll to zoom.</summary>
    public class OrbitCamera : MonoBehaviour
    {
        public Vector3 target = new Vector3(0f, 1f, 0f);
        public float distance = 7f;
        public float yaw = -25f;
        public float pitch = 20f;
        public float minDistance = 2f;
        public float maxDistance = 30f;
        public float orbitSpeed = 0.25f;
        public float panSpeed = 0.004f;

        private void LateUpdate()
        {
            if (SandboxInput.RightHeld)
            {
                Vector2 delta = SandboxInput.MouseDelta;
                yaw += delta.x * orbitSpeed;
                pitch = Mathf.Clamp(pitch - delta.y * orbitSpeed, -10f, 85f);
            }

            if (SandboxInput.MiddleHeld)
            {
                Vector2 delta = SandboxInput.MouseDelta * (panSpeed * distance);
                target -= transform.right * delta.x + transform.up * delta.y;
            }

            // (Over the panel the wheel scrolls the panel.)
            float scroll = SandboxInput.OverPanel ? 0f : SandboxInput.Scroll;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance = Mathf.Clamp(distance * Mathf.Pow(0.88f, scroll), minDistance, maxDistance);
            }

            Apply();
        }

        public void Apply()
        {
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 position = target - rotation * Vector3.forward * distance;
            if (shake > 1e-4f)
            {
                // A short, decaying jolt: a few centimetres, scaled with how far out the camera is.
                float t = Time.unscaledTime * 38f;
                Vector3 jolt = new Vector3(Mathf.PerlinNoise(t, 0.3f) - 0.5f, Mathf.PerlinNoise(0.7f, t) - 0.5f, 0f);
                position += rotation * jolt * (shake * 0.06f * distance / 7f);
                shake = Mathf.MoveTowards(shake, 0f, Time.unscaledDeltaTime / ShakeTime);
            }

            if (kick.sqrMagnitude > 1e-8f)
            {
                // (A combo's big one: the camera is knocked a few centimetres the way the blow went and comes back.)
                position += kick * (distance / 7f);
                kick = Vector3.Lerp(kick, Vector3.zero, 1f - Mathf.Exp(-Time.unscaledDeltaTime / KickTime));
            }

            transform.SetPositionAndRotation(position, rotation);
        }

        private Vector3 kick;

        /// <summary>The kick dies away over about this long (seconds of the real clock).</summary>
        private const float KickTime = 0.09f;

        /// <summary>A kick of the camera along <paramref name="way"/> (the world's), 0 to 1: the big one of a combo landing (the combos plan's step 6).</summary>
        public void Kick(Vector3 way, float amount)
        {
            if (way.sqrMagnitude > 1e-6f)
            {
                kick = way.normalized * (0.22f * Mathf.Clamp01(amount));
                shake = Mathf.Clamp01(Mathf.Max(shake, 0.6f * amount));
            }
        }

        private void OnEnable()
        {
            PhysicsBattler.Locomotion.ComboShow.Kicked += Kick;
        }

        private void OnDisable()
        {
            PhysicsBattler.Locomotion.ComboShow.Kicked -= Kick;
        }

        private float shake;

        /// <summary>Seconds a full shake takes to die away.</summary>
        private const float ShakeTime = 0.25f;

        /// <summary>Shakes on hits are on (the sandbox panel toggles it).</summary>
        public static bool ShakeOnHits = true;

        /// <summary>A jolt of the camera (0 to 1), for a heavy hit.</summary>
        public void Shake(float amount)
        {
            if (ShakeOnHits)
            {
                shake = Mathf.Clamp01(Mathf.Max(shake, amount));
            }
        }
    }
}
