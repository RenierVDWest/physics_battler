using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PhysicsBattler.Sandbox
{
    /// <summary>
    /// Mouse and keyboard for the sandbox, through the Input System package when it is active and
    /// the old Input Manager otherwise, so the sandbox works whichever is enabled.
    /// </summary>
    public static class SandboxInput
    {
#if ENABLE_INPUT_SYSTEM
        public static Vector2 MousePosition => Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        public static Vector2 MouseDelta => Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        /// <summary>
        /// About ±1 per wheel notch. The Input System reports scroll normalized (±1 a notch) by default
        /// since 1.9, or in platform units (±120 on Windows) if the project keeps the old behaviour.
        /// </summary>
        public static float Scroll
        {
            get
            {
                float y = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
                if (Mathf.Abs(y) > 3f)
                {
                    y /= 120f;
                }

                return Mathf.Clamp(y, -3f, 3f);
            }
        }
        public static bool LeftPressed => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        public static bool RightHeld => Mouse.current != null && Mouse.current.rightButton.isPressed;
        public static bool MiddleHeld => Mouse.current != null && Mouse.current.middleButton.isPressed;

        public static bool KeyPressed(KeyCode key)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            switch (key)
            {
                case KeyCode.Space: return keyboard.spaceKey.wasPressedThisFrame;
                case KeyCode.R: return keyboard.rKey.wasPressedThisFrame;
                case KeyCode.P: return keyboard.pKey.wasPressedThisFrame;
                case KeyCode.G: return keyboard.gKey.wasPressedThisFrame;
                case KeyCode.H: return keyboard.hKey.wasPressedThisFrame;
                case KeyCode.Period: return keyboard.periodKey.wasPressedThisFrame;
                case KeyCode.M: return keyboard.mKey.wasPressedThisFrame;
                default: return false;
            }
        }
#else
        public static Vector2 MousePosition => Input.mousePosition;
        public static Vector2 MouseDelta => new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 10f;
        public static float Scroll => Input.mouseScrollDelta.y;
        public static bool LeftPressed => Input.GetMouseButtonDown(0);
        public static bool RightHeld => Input.GetMouseButton(1);
        public static bool MiddleHeld => Input.GetMouseButton(2);
        public static bool KeyPressed(KeyCode key) => Input.GetKeyDown(key);
#endif
    }
}
