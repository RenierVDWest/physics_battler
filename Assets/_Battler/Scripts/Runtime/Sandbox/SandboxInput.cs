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
        /// <summary>The mouse is over the sandbox's panel (set by the panel as it is drawn): its wheel scrolls the panel then, and does not zoom the camera.</summary>
        public static bool OverPanel;

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
                case KeyCode.Comma: return keyboard.commaKey.wasPressedThisFrame;
                case KeyCode.M: return keyboard.mKey.wasPressedThisFrame;
                case KeyCode.T: return keyboard.tKey.wasPressedThisFrame;
                case KeyCode.F: return keyboard.fKey.wasPressedThisFrame;
                case KeyCode.B: return keyboard.bKey.wasPressedThisFrame;
                case KeyCode.L: return keyboard.lKey.wasPressedThisFrame;
                case KeyCode.K: return keyboard.kKey.wasPressedThisFrame;
                case KeyCode.D: return keyboard.dKey.wasPressedThisFrame;
                case KeyCode.J: return keyboard.jKey.wasPressedThisFrame;
                case KeyCode.C: return keyboard.cKey.wasPressedThisFrame;
                case KeyCode.V: return keyboard.vKey.wasPressedThisFrame;
                case KeyCode.N: return keyboard.nKey.wasPressedThisFrame;
                case KeyCode.U: return keyboard.uKey.wasPressedThisFrame;
                case KeyCode.Y: return keyboard.yKey.wasPressedThisFrame;
                case KeyCode.O: return keyboard.oKey.wasPressedThisFrame;
                case KeyCode.I: return keyboard.iKey.wasPressedThisFrame;
                case KeyCode.X: return keyboard.xKey.wasPressedThisFrame;
                case KeyCode.Z: return keyboard.zKey.wasPressedThisFrame;
                case KeyCode.Alpha1: return keyboard.digit1Key.wasPressedThisFrame;
                case KeyCode.Alpha2: return keyboard.digit2Key.wasPressedThisFrame;
                case KeyCode.Alpha3: return keyboard.digit3Key.wasPressedThisFrame;
                case KeyCode.Alpha4: return keyboard.digit4Key.wasPressedThisFrame;
                case KeyCode.Alpha5: return keyboard.digit5Key.wasPressedThisFrame;
                case KeyCode.Alpha6: return keyboard.digit6Key.wasPressedThisFrame;
                default: return false;
            }
        }

        /// <summary>A key held down (the locomotion lab's driving keys).</summary>
        public static bool KeyHeld(KeyCode key)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            switch (key)
            {
                case KeyCode.W: return keyboard.wKey.isPressed;
                case KeyCode.A: return keyboard.aKey.isPressed;
                case KeyCode.S: return keyboard.sKey.isPressed;
                case KeyCode.D: return keyboard.dKey.isPressed;
                case KeyCode.Q: return keyboard.qKey.isPressed;
                case KeyCode.E: return keyboard.eKey.isPressed;
                case KeyCode.LeftShift: return keyboard.leftShiftKey.isPressed;
                case KeyCode.Space: return keyboard.spaceKey.isPressed;
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
        public static bool KeyHeld(KeyCode key) => Input.GetKey(key);
#endif
    }
}
