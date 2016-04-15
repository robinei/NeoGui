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

        public void CopyFrom(InputState state)
        {
            Time = state.Time;
            MousePos = state.MousePos;
            for (var i = 0; i < 3; ++i) {
                MouseButtonDown[i] = state.MouseButtonDown[i];
            }
        }
    }

    public class InputContext
    {
        private readonly NeoGuiContext context;

        private InputState curr = new InputState();
        private InputState prev = new InputState();
        private bool hasSetStateBefore;

        private readonly bool[] mouseButtonPressConsumed = new bool[3];
        private bool dragPending;

        
        internal InputContext(NeoGuiContext context)
        {
            this.context = context;
        }

        public void SetNewState(InputState newState)
        {
            var temp = curr;
            curr = prev;
            prev = temp;

            curr.CopyFrom(newState);
            if (!hasSetStateBefore) {
                prev.CopyFrom(newState);
                hasSetStateBefore = true;
            }
        }
        

        public double Time => curr.Time;
        public double TimeDelta => Time - prev.Time;

        public Vec2 MousePos => curr.MousePos;
        public Vec2 MouseDelta => MousePos - prev.MousePos;
        public bool DidMouseMove => MouseDelta.SqrLength > 0;
        
        
        public bool IsDragging { get; private set; }
        public Vec2 DragOrigin { get; private set; }
        public Vec2 TrueDragOrigin { get; private set; }
        public Vec2 DragPos => MousePos;
        public Vec2 DragRemainder { get; set; }
        public int DragRemainderUses { get; set; }
        

        public void PreUiUpdate()
        {
            DragRemainder = IsDragging ? DragPos - DragOrigin : Vec2.Zero;
            DragRemainderUses = 0;
            
            for (var i = 0; i < 3; ++i) {
                mouseButtonPressConsumed[i] = false;
            }
        }

        public void PostUiUpdate()
        {
            if (WasMouseButtonPressed(MouseButton.Left)) {
                // it was pressed this frame, and no-one consumed it
                dragPending = true;
                TrueDragOrigin = MousePos;
            }

            if (WasMouseButtonReleased(MouseButton.Left)) {
                dragPending = false;
                IsDragging = false;
                TrueDragOrigin = Vec2.Zero;
                DragOrigin = Vec2.Zero;
            }

            if (dragPending && (MousePos - TrueDragOrigin).Length > 5) {
                dragPending = false;
                IsDragging = true;
                DragOrigin = MousePos;
            }
        }


        public bool IsMouseButtonDown(MouseButton button) => curr.MouseButtonDown[(int)button];

        public bool WasMouseButtonPressed(MouseButton button, bool respectIfConsumed = true)
        {
            return curr.MouseButtonDown[(int)button] &&
                   !prev.MouseButtonDown[(int)button] &&
                   (!respectIfConsumed || !mouseButtonPressConsumed[(int)button]);
        }
        public void ConsumeMouseButtonPressed(MouseButton button)
        {
            mouseButtonPressConsumed[(int)button] = true;
        }

        public bool WasMouseButtonReleased(MouseButton button)
        {
            return prev.MouseButtonDown[(int)button] && !curr.MouseButtonDown[(int)button];
        }
    }
}
