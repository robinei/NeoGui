using System.Diagnostics;

namespace NeoGui.Core
{
    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }

    public class InputState
    {
        public double Time;
        public Vec2 MousePos;
        public readonly bool[] MouseButtonDown = new bool[3];
    }

    public class InputContext
    {
        private readonly NeoGuiContext context;
        private readonly InputTracker tracker;
        private InputContext prevInput;

        private readonly bool[] mouseButtonDown = new bool[3];
        private readonly bool[] mouseButtonPressConsumed = new bool[3];
        private readonly bool[] mouseButtonReleaseConsumed = new bool[3];


        public double Time { get; private set; }
        public Vec2 MousePos { get; private set; }

        internal InputContext(NeoGuiContext context, InputTracker tracker)
        {
            this.context = context;
            this.tracker = tracker;
        }

        internal void SetRawState(InputContext prev, InputState inputState)
        {
            prevInput = prev;
            Time = inputState.Time;
            MousePos = inputState.MousePos;
            for (var i = 0; i < 3; ++i) {
                mouseButtonDown[i] = inputState.MouseButtonDown[i];
                mouseButtonPressConsumed[i] = false;
                mouseButtonReleaseConsumed[i] = false;
            }
        }

        public Vec2 MouseDelta => MousePos - prevInput.MousePos;
        public bool DidMouseMove => MouseDelta.SqrLength > 0;

        public bool IsDragging => tracker.IsDragging;
        public Vec2 DragOrigin => tracker.DragOrigin;
        public Vec2 TrueDragOrigin => tracker.TrueDragOrigin;
        public Vec2 DragPos => tracker.DragPos;
        public Vec2 DragVector
        {
            get { return tracker.DragVector; }
            set { tracker.DragVector = value; }
        }

        public bool IsMouseButtonDown(MouseButton button) => mouseButtonDown[(int)button];

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


    internal class InputTracker
    {
        private readonly NeoGuiContext context;

        private bool dragPending;

        public InputContext Input { get; set; }

        public InputTracker(NeoGuiContext context)
        {
            this.context = context;
        }

        public void PreUiUpdate()
        {
            Debug.Assert(Input != null);
            
            if (Input.WasMouseButtonReleased(MouseButton.Left)) {
                dragPending = false;
                IsDragging = false;
                TrueDragOrigin = Vec2.Zero;
                DragOrigin = Vec2.Zero;
            }

            if (dragPending && (Input.MousePos - TrueDragOrigin).Length > 5) {
                dragPending = false;
                IsDragging = true;
                DragOrigin = Input.MousePos;
            }

            DragVector = IsDragging ? DragPos - DragOrigin : Vec2.Zero;
        }

        public void PostUiUpdate()
        {
            Debug.Assert(Input != null);
            if (Input.WasMouseButtonPressed(MouseButton.Left)) {
                // it was pressed this frame, and no-one consumed it
                dragPending = true;
                TrueDragOrigin = Input.MousePos;
            }
        }

        public bool IsDragging { get; private set; }
        public Vec2 DragOrigin { get; private set; }
        public Vec2 TrueDragOrigin { get; private set; }
        public Vec2 DragPos => Input.MousePos;
        public Vec2 DragVector { get; set; }
    }
}
