using System;
using System.Diagnostics;
using NeoGui.Core;

namespace NeoGui.Toolkit
{
    public class ScrollAreaState
    {
        public ScrollAreaFlags Flags;
        public Vec2 Pos;
        public Vec2 ClientSize; // not used here, but useful in VirtualList for example

        public readonly ScrollAreaAxisMode[] Mode = new ScrollAreaAxisMode[2];
        
        public bool IsDragging;
        public Vec2 OrigPos; // value of Pos at start of drag
        public Vec2 DriftSpeed; // drag speed at end of drag. decays over time
        public Vec2 DragWanted;
        
        public readonly double[] AnimTimeStart = new double[2];
        public readonly double[] AnimTimeEnd = new double[2];
        public readonly float[] AnimPosInfo = new float[2];
    }

    [Flags]
    public enum ScrollAreaFlags
    {
        BounceX = 1,
        BounceY = 2,
        FillX = 4, // implies that the width of the content panel will be set to equal to that of the scroll area. so no scrolling
        FillY = 8
    }

    public enum ScrollAreaAxisMode
    {
        Idle,
        Drag,
        Drift,
        Bounce,
        Debounce
    }

    public static class ScrollArea
    {
        private const double BounceInterval = 0.2;
        private const double DebounceInterval = 0.1;

        public static Element Create(
            Element parent, 
            ScrollAreaFlags flags = ScrollAreaFlags.BounceX | ScrollAreaFlags.BounceY,
            Func<Element, Element>? contentCreator = null,
            object? key = null,
            StateDomain? domain = null)
        {
            var scrollArea = Element.Create(parent, key, domain);
            scrollArea.ClipContent = true;
            scrollArea.Layout = e => Layout(e);
            scrollArea.OnInserted(e => OnInserted(e));
            scrollArea.OnDepthDescent(e => OnDepthDescent(e));
            var state = scrollArea.GetOrCreateState<ScrollAreaState>();
            state.Flags = flags;

            var content = contentCreator?.Invoke(scrollArea) ?? Element.Create(scrollArea);
            content.Name = "ScrollArea.Content";
            content.ClipContent = true;

            var overlay = Element.Create(scrollArea);
            overlay.Name = "ScrollArea.Overlay";
            overlay.Draw = dc => DrawOverlay(dc);
            overlay.ZIndex = 1;

            return scrollArea;
        }

        public static Element GetContentPanel(Element scrollArea)
        {
            return scrollArea.FindChild(e => e.Name == "ScrollArea.Content") ?? throw new Exception("cannot find ScrollArea.Content");
        }

        private static Element GetOverlayPanel(Element scrollArea)
        {
            return scrollArea.FindChild(e => e.Name == "ScrollArea.Overlay") ?? throw new Exception("cannot find ScrollArea.Overlay");
        }

        public static void Layout(Element scrollArea)
        {
            var content = GetContentPanel(scrollArea);
            var overlay = GetOverlayPanel(scrollArea);
            var state = scrollArea.GetOrCreateState<ScrollAreaState>();
            content.Pos = state.Pos;
            if ((state.Flags & ScrollAreaFlags.FillX) != 0) { content.Width = scrollArea.Width; }
            if ((state.Flags & ScrollAreaFlags.FillY) != 0) { content.Height = scrollArea.Height; }
            overlay.Rect = new Rect(scrollArea.Size);
        }

        private static void OnInserted(Element scrollArea)
        {
            var content = GetContentPanel(scrollArea);
            var state = scrollArea.GetOrCreateState<ScrollAreaState>();
            state.Pos = ClampToBounds(state.Pos, content.Size, scrollArea.Size);
            state.IsDragging = false;
            state.Mode[0] = ScrollAreaAxisMode.Idle;
            state.Mode[1] = ScrollAreaAxisMode.Idle;
        }

        private static void OnDepthDescent(Element scrollArea)
        {
            var input = scrollArea.Context.Input;
            var state = scrollArea.GetOrCreateState<ScrollAreaState>();
            var content = GetContentPanel(scrollArea);

            state.ClientSize = scrollArea.Size;

            if (input.DidDragStart() && !scrollArea.Disabled && scrollArea.HitTest(input.TrueDragOrigin)) {
                state.IsDragging = true;
                state.Mode[0] = ScrollAreaAxisMode.Drag;
                state.Mode[1] = ScrollAreaAxisMode.Drag;
                state.OrigPos = state.Pos;
            }

            if (state.IsDragging) {
                if (!input.IsDragging || scrollArea.Disabled) {
                    state.IsDragging = false;
                } else {
                    // apply the whole DragRemainder
                    var pos = state.OrigPos + input.DragRemainder;
                
                    // move pos back if we went out of bounds
                    pos = ClampToBounds(pos, content.Size, scrollArea.Size);
                    
                    state.DragWanted = input.DragRemainder;
                    var dragUsed = (pos - state.OrigPos);

                    // subtract the part of the DragRemainder that we "used".
                    // what's left can be "used" by someone further up the hierarchy
                    input.DragRemainder -= dragUsed;
                }
            }

            scrollArea.OnPassFinished(e => Update(e));
        }

        private static void Update(Element scrollArea)
        {
            UpdateAxis(scrollArea, 0);
            UpdateAxis(scrollArea, 1);
        }

        private static void UpdateAxis(Element scrollArea, int axis)
        {
            var input = scrollArea.Context.Input;
            var state = scrollArea.GetOrCreateState<ScrollAreaState>();
            var content = GetContentPanel(scrollArea);
            // TODO: handle scale

            switch (state.Mode[axis]) {
            case ScrollAreaAxisMode.Idle: {
                state.Pos[axis] = ClampAxisToBounds(axis, state.Pos, content.Size, scrollArea.Size)[axis];
                break;
            }
            case ScrollAreaAxisMode.Drag:
                if (state.IsDragging) {
                    var posVec = state.OrigPos + state.DragWanted;
                    var boundedPos = ClampToBounds(posVec, content.Size, scrollArea.Size)[axis];
                    var pos = posVec[axis];
                    if (Math.Abs(pos - boundedPos) > 0) {
                        var fullDragAbs = Math.Abs(input.DragPos[axis] - input.DragOrigin[axis]);
                        var dragWantedAbs = Math.Abs(state.DragWanted[axis]);
                        var shareFactor = fullDragAbs > 0 ? dragWantedAbs / fullDragAbs : 1.0f;
                        var displace = input.DragRemainder[axis] * shareFactor;
                        displace *= 1.0f / (float)Math.Max(Math.Sqrt(Math.Abs(displace)), 1.0);
                        pos = boundedPos + displace;
                    }
                    state.Pos[axis] = pos;
                } else {
                    if (state.Pos[axis] > 0 || state.Pos[axis] + content.Size[axis] < scrollArea.Size[axis]) {
                        state.AnimPosInfo[axis] = state.Pos[axis]; // start pos
                        state.AnimTimeStart[axis] = input.Time;
                        state.AnimTimeEnd[axis] = input.Time + DebounceInterval;
                        state.Mode[axis] = ScrollAreaAxisMode.Debounce;
                    } else {
                        if (Math.Abs(state.DragWanted[axis]) > 0) {
                            state.DriftSpeed[axis] = input.DragSpeed[axis];
                            state.Mode[axis] = ScrollAreaAxisMode.Drift;
                        } else {
                            state.Mode[axis] = ScrollAreaAxisMode.Idle;
                        }
                    }
                }
                break;
            case ScrollAreaAxisMode.Drift:
                state.Pos[axis] += (float)(state.DriftSpeed[axis] * input.TimeDelta);

                state.DriftSpeed[axis] -= (float)(state.DriftSpeed[axis] * input.TimeDelta);
                if (Math.Abs(state.DriftSpeed[axis]) < 10) {
                    state.DriftSpeed[axis] -= (float)(state.DriftSpeed[axis] * input.TimeDelta * 2);
                }
                if (Math.Abs(state.DriftSpeed[axis]) < 2) {
                    state.DriftSpeed[axis] = 0;
                }

                if (state.Pos[axis] > 0) {
                    state.Pos[axis] = 0;
                    state.AnimPosInfo[axis] = (float)Math.Sqrt(Math.Abs(state.DriftSpeed[axis]) * BounceInterval); // delta
                    state.AnimTimeStart[axis] = input.Time;
                    state.AnimTimeEnd[axis] = input.Time + BounceInterval;
                    state.Mode[axis] = ScrollAreaAxisMode.Bounce;
                } else if (state.Pos[axis] + content.Size[axis] < scrollArea.Size[axis]) {
                    state.Pos[axis] = scrollArea.Size[axis] - content.Size[axis];
                    state.AnimPosInfo[axis] = -(float)Math.Sqrt(Math.Abs(state.DriftSpeed[axis]) * BounceInterval); // delta
                    state.AnimTimeStart[axis] = input.Time;
                    state.AnimTimeEnd[axis] = input.Time + BounceInterval;
                    state.Mode[axis] = ScrollAreaAxisMode.Bounce;
                }
                break;
            case ScrollAreaAxisMode.Bounce: {
                var delta = state.AnimPosInfo[axis];
                float startPos = 0, endPos = delta;
                if (delta < 0)  {
                    startPos = scrollArea.Size[axis] - content.Size[axis];
                    endPos = startPos + delta;
                }
                var t = Util.NormalizeInInterval(input.Time, state.AnimTimeStart[axis], state.AnimTimeEnd[axis]);
                t = 2 * Util.Sigmoid(5 * t) - 0.986;
                state.Pos[axis] = (float)(startPos + (endPos - startPos) * t);
                if (t >= 1) {
                    state.AnimPosInfo[axis] = state.Pos[axis]; // start pos
                    state.AnimTimeStart[axis] = input.Time;
                    state.AnimTimeEnd[axis] = input.Time + DebounceInterval;
                    state.Mode[axis] = ScrollAreaAxisMode.Debounce;
                }
                break;
            }
            case ScrollAreaAxisMode.Debounce: {
                float startPos = state.AnimPosInfo[axis], endPos = 0;
                if (startPos < 0) {
                    endPos = scrollArea.Size[axis] - content.Size[axis];
                    if (endPos > 0) {
                        endPos = 0;
                    }
                }
                var t = Util.NormalizeInInterval(input.Time, state.AnimTimeStart[axis], state.AnimTimeEnd[axis]);
                t = 2 * Util.Sigmoid(5 * t - 5) + 0.0001;
                state.Pos[axis] = (float)(startPos + (endPos - startPos) * t);
                if (t >= 1) {
                    state.Mode[axis] = ScrollAreaAxisMode.Idle;
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
            }

            if ((state.Flags & AxisBounceFlags[axis]) == 0) {
                state.Pos[axis] = ClampAxisToBounds(axis, state.Pos, content.Size, scrollArea.Size)[axis];
            }
        }

        private static readonly ScrollAreaFlags[] AxisBounceFlags = {ScrollAreaFlags.BounceX, ScrollAreaFlags.BounceY};

        private static float ClampToBounds(float pos, float contentSize, float clientSize)
        {
            if (pos > 0) {
                return 0;
            }
            if (pos + contentSize < clientSize) {
                if (clientSize - contentSize > 0) {
                    return 0;
                }
                return clientSize - contentSize;
            }
            return pos;
        }

        private static Vec2 ClampToBounds(Vec2 pos, Vec2 contentSize, Vec2 clientSize)
        {
            pos.X = ClampToBounds(pos.X, contentSize.X, clientSize.X);
            pos.Y = ClampToBounds(pos.Y, contentSize.Y, clientSize.Y);
            return pos;
        }

        private static Vec2 ClampAxisToBounds(int axis, Vec2 pos, Vec2 contentSize, Vec2 clientSize)
        {
            if (axis == 0) {
                pos.X = ClampToBounds(pos.X, contentSize.X, clientSize.X);
            } else {
                pos.Y = ClampToBounds(pos.Y, contentSize.Y, clientSize.Y);
            }
            return pos;
        }

        private static void DrawOverlay(DrawContext dc)
        {
            var overlay = dc.Target;
            var scrollArea = overlay.Parent;
            var content = GetContentPanel(scrollArea);

            if (content.Width > scrollArea.Width) {
                var length = Math.Max(20, scrollArea.Width * scrollArea.Width / content.Width);
                var offset = (scrollArea.Width - length) * -content.X / (content.Width - scrollArea.Width);
                dc.SolidRect(new Rect(offset, scrollArea.Height - 5, length, 5), new Color(0, 0, 0, 64));
            }

            if (content.Height > scrollArea.Height) {
                var length = Math.Max(20, scrollArea.Height * scrollArea.Height / content.Height);
                var offset = (scrollArea.Height - length) * -content.Y / (content.Height - scrollArea.Height);
                dc.SolidRect(new Rect(scrollArea.Width - 5, offset, 5, length), new Color(0, 0, 0, 64));
            }
        }
    }
}
