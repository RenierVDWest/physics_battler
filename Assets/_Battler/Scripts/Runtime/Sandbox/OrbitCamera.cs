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

            float scroll = SandboxInput.Scroll;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance = Mathf.Clamp(distance * Mathf.Pow(0.88f, scroll), minDistance, maxDistance);
            }

            Apply();
        }

        public void Apply()
        {
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.SetPositionAndRotation(target - rotation * Vector3.forward * distance, rotation);
        }
    }
}
