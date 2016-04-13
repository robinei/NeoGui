namespace NeoGui.Core
{
    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }

    public class RawInputState
    {
        public double Time;
        public Vec2 MousePos;
        public readonly bool[] MouseButtonDown = new bool[3];
    }

    public class InputState
    {
        private InputState prevInput;
        private readonly bool[] mouseButtonDown = new bool[3];
        private readonly bool[] mouseButtonPressConsumed = new bool[3];
        private readonly bool[] mouseButtonReleaseConsumed = new bool[3];

        public double Time { get; private set; }
        public Vec2 MousePos { get; private set; }

        internal void Reset(InputState prev, RawInputState rawInput)
        {
            prevInput = prev;
            Time = rawInput.Time;
            MousePos = rawInput.MousePos;
            for (var i = 0; i < 3; ++i) {
                mouseButtonDown[i] = rawInput.MouseButtonDown[i];
                mouseButtonPressConsumed[i] = false;
                mouseButtonReleaseConsumed[i] = false;
            }
        }

        public Vec2 MouseDelta => MousePos - prevInput.MousePos;
        public bool DidMouseMove => MouseDelta.SqrLength > 0;

        public bool IsMouseButtonDown(MouseButton button)
        {
            return mouseButtonDown[(int)button];
        }

        public bool WasMouseButtonPressed(MouseButton button, bool respectIfConsumed = true)
        {
            return IsMouseButtonDown(button) &&
                   !prevInput.IsMouseButtonDown(button) &&
                   (!respectIfConsumed || !mouseButtonPressConsumed[(int)button]);
        }
        public void ConsumeMouseButtonPressed(MouseButton button)
        {
            mouseButtonPressConsumed[(int)button] = true;
        }

        public bool WasMouseButtonReleased(MouseButton button, bool respectIfConsumed = true)
        {
            return !IsMouseButtonDown(button) &&
                   prevInput.IsMouseButtonDown(button) &&
                   (!respectIfConsumed || !mouseButtonReleaseConsumed[(int)button]);
        }
        public void ConsumeMouseButtonReleased(MouseButton button)
        {
            mouseButtonReleaseConsumed[(int)button] = true;
        }
    }
}
