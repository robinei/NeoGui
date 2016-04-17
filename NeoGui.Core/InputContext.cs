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


        public bool DidDragStart(bool respectIfConsumed = true) => dragStarted && (!respectIfConsumed || !dragStartedConsumed);
        public void ConsumeDragStart()
        {
            dragStartedConsumed = true;
        }

        public bool IsDragging { get; private set; }
        public Vec2 DragOrigin { get; private set; }
        public Vec2 TrueDragOrigin { get; private set; }
        public Vec2 DragPos => MousePos;
        public Vec2 DragSpeed { get; private set; }
        
        /// <summary>
        /// Set to DragPos - DragOrigin at the start of each update.
        /// Everyone who uses part of the drag in some way where it makes sense to just use part of it
        /// subtracts the part they need from DragRemainder.
        /// </summary>
        public Vec2 DragRemainder { get; set; }

        
        private bool dragStarted;
        private bool dragStartedConsumed;
        private bool dragPending;
        private double dragSpeedSampleStartTime;
        private Vec2 dragSpeedSampleStartPos;
        private bool hasGottenFirstDragSpeedSample;

        internal void PreUiUpdate()
        {
            dragStarted = false;
            dragStartedConsumed = false;

            if (WasMouseButtonReleased(MouseButton.Left)) {
                SampleDragSpeed();
                dragPending = false;
                IsDragging = false;
            }

            if (dragPending && (MousePos - TrueDragOrigin).Length > 5) {
                dragStarted = true;
                dragPending = false;
                IsDragging = true;
                DragOrigin = MousePos;
            }

            DragRemainder = IsDragging ? DragPos - DragOrigin : Vec2.Zero;
            if ((dragPending || IsDragging) && Time - dragSpeedSampleStartTime > 0.05) {
                SampleDragSpeed();
            }
            
            for (var i = 0; i < 3; ++i) {
                mouseButtonPressConsumed[i] = false;
            }
        }

        internal void PostUiUpdate()
        {
            if (WasMouseButtonPressed(MouseButton.Left)) {
                // it was pressed this frame, and no-one consumed it
                dragPending = true;
                dragSpeedSampleStartTime = Time;
                dragSpeedSampleStartPos = MousePos;
                hasGottenFirstDragSpeedSample = false;
                DragSpeed = Vec2.Zero;
                TrueDragOrigin = MousePos;
            }
        }

        private void SampleDragSpeed()
        {
            var speed = (DragPos - dragSpeedSampleStartPos) * (float)(1.0 / (Time - dragSpeedSampleStartTime));
            if (hasGottenFirstDragSpeedSample) {
                DragSpeed = DragSpeed * 0.75f + speed * 0.25f;
            } else {
                DragSpeed = speed;
                hasGottenFirstDragSpeedSample = true;
            }
            dragSpeedSampleStartTime = Time;
            dragSpeedSampleStartPos = MousePos;
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
