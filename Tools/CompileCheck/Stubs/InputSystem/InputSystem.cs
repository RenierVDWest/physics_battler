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
        public KeyControl fKey { get; } = new KeyControl();
        public KeyControl bKey { get; } = new KeyControl();
        public KeyControl lKey { get; } = new KeyControl();
        public KeyControl kKey { get; } = new KeyControl();
        public KeyControl dKey { get; } = new KeyControl();
        public KeyControl jKey { get; } = new KeyControl();
        public KeyControl cKey { get; } = new KeyControl();
        public KeyControl vKey { get; } = new KeyControl();
        public KeyControl nKey { get; } = new KeyControl();
        public KeyControl uKey { get; } = new KeyControl();
        public KeyControl yKey { get; } = new KeyControl();
        public KeyControl zKey { get; } = new KeyControl();
        public KeyControl digit1Key { get; } = new KeyControl();
        public KeyControl oKey { get; } = new KeyControl();
        public KeyControl iKey { get; } = new KeyControl();
        public KeyControl xKey { get; } = new KeyControl();
        public KeyControl commaKey { get; } = new KeyControl();
        public KeyControl wKey { get; } = new KeyControl();
        public KeyControl aKey { get; } = new KeyControl();
        public KeyControl sKey { get; } = new KeyControl();
        public KeyControl qKey { get; } = new KeyControl();
        public KeyControl eKey { get; } = new KeyControl();
        public KeyControl leftShiftKey { get; } = new KeyControl();
    }
}
