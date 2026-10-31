// Compile-check stubs for the Input System package members the sandbox uses.
namespace UnityEngine.InputSystem.Controls
{
    public class ButtonControl
    {
        public bool isPressed => false;
        public bool wasPressedThisFrame => false;
        public bool wasReleasedThisFrame => false;
    }

    public class KeyControl : ButtonControl { }

    public class Vector2Control
    {
        public Vector2 ReadValue() => Vector2.zero;
    }
}

namespace UnityEngine.InputSystem
{
    using UnityEngine.InputSystem.Controls;

    public class Mouse
    {
        public static Mouse current => null;
        public Vector2Control position { get; } = new Vector2Control();
        public Vector2Control delta { get; } = new Vector2Control();
        public Vector2Control scroll { get; } = new Vector2Control();
        public ButtonControl leftButton { get; } = new ButtonControl();
        public ButtonControl rightButton { get; } = new ButtonControl();
        public ButtonControl middleButton { get; } = new ButtonControl();
    }

    public class Keyboard
    {
        public static Keyboard current => null;
        public KeyControl spaceKey { get; } = new KeyControl();
        public KeyControl rKey { get; } = new KeyControl();
        public KeyControl pKey { get; } = new KeyControl();
        public KeyControl gKey { get; } = new KeyControl();
        public KeyControl hKey { get; } = new KeyControl();
        public KeyControl periodKey { get; } = new KeyControl();
        public KeyControl mKey { get; } = new KeyControl();
        public KeyControl tKey { get; } = new KeyControl();
    }
}
