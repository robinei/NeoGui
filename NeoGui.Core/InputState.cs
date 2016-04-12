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

        public double Time { get; private set; }
        public Vec2 MousePos { get; private set; }

        internal void Reset(InputState prev, RawInputState rawInput)
        {
            prevInput = prev;
            Time = rawInput.Time;
            MousePos = rawInput.MousePos;
            mouseButtonDown[0] = rawInput.MouseButtonDown[0];
            mouseButtonDown[1] = rawInput.MouseButtonDown[1];
            mouseButtonDown[2] = rawInput.MouseButtonDown[2];
        }

        public Vec2 MouseDelta => MousePos - prevInput.MousePos;
        public bool DidMouseMove => MouseDelta.SqrLength > 0;

        public bool IsMouseButtonDown(MouseButton button)
        {
            return mouseButtonDown[(int)button];
        }
        public bool WasMouseButtonPressed(MouseButton button)
        {
            return IsMouseButtonDown(button) && !prevInput.IsMouseButtonDown(button);
        }
        public bool WasMouseButtonReleased(MouseButton button)
        {
            return !IsMouseButtonDown(button) && prevInput.IsMouseButtonDown(button);
        }
    }
}
