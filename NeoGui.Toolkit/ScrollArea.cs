using System;
using System.Diagnostics;
using NeoGui.Core;

namespace NeoGui.Toolkit
{
    public class ScrollAreaState
    {
        public ScrollAreaFlags Flags;

        public Vec2 Overflow;
        public Vec2 Pos;
        public Vec2 OrigPos;
        public bool IsDragging;

        public Vec2 ClientSize; // not used here, but useful in VirtualList for example
    }

    [Flags]
    public enum ScrollAreaFlags
    {
        OverDragX = 1,
        OverDragY = 2
    }

    public static class ScrollArea
    {
        public static Element Create(
            Element parent, 
            ScrollAreaFlags flags = ScrollAreaFlags.OverDragX | ScrollAreaFlags.OverDragY,
            object key = null)
        {
            var scrollArea = Element.Create(parent, key);
            scrollArea.Name = "ScrollArea";
            scrollArea.ClipContent = true;
            scrollArea.Layout = Layout;
            scrollArea.OnDepthDescent(OnDepthDescent);
            var state = scrollArea.GetOrCreateState<ScrollAreaState>();
            state.Flags = flags;

            var content = Element.Create(scrollArea);
            content.Name = "ScrollArea.Content";

            var overlay = Element.Create(scrollArea);
            overlay.Name = "ScrollArea.Overlay";
            overlay.Draw = DrawOverlay;
            overlay.ZIndex = 1;

            return scrollArea;
        }

        public static Element GetContentPanel(Element scrollArea)
        {
            var e = scrollArea.FirstChild.NextSibling;
            Debug.Assert(e.Name == "ScrollArea.Content");
            return e;
        }

        private static Element GetOverlayPanel(Element scrollArea)
        {
            var e = scrollArea.FirstChild;
            Debug.Assert(e.Name == "ScrollArea.Overlay");
            return e;
        }

        public static void Layout(Element scrollArea)
        {
            var content = GetContentPanel(scrollArea);
            var overlay = GetOverlayPanel(scrollArea);
            var state = scrollArea.GetOrCreateState<ScrollAreaState>();
            content.Pos = state.Pos + state.Overflow;
            overlay.Rect = new Rect(scrollArea.Size);
        }

        private static void OnDepthDescent(Element scrollArea)
        {
            var state = scrollArea.GetOrCreateState<ScrollAreaState>();
            state.ClientSize = scrollArea.Size;
            if (!scrollArea.Enabled) {
                state.IsDragging = false;
                return;
            }
            var input = scrollArea.Context.Input;
            if (state.IsDragging) {
                var content = GetContentPanel(scrollArea);

                if (!input.IsDragging) {
                    state.IsDragging = false;
                    return;
                }
                
                // apply the whole DragRemainder
                var scale = scrollArea.ToLocalScale(1.0f);
                var pos = state.OrigPos + input.DragRemainder * scale;
                
                // move pos back if we went out of bounds
                if (pos.X + content.Width < scrollArea.Width) { pos.X = scrollArea.Width - content.Width; }
                if (pos.Y + content.Height < scrollArea.Height) { pos.Y = scrollArea.Height - content.Height; }
                if (pos.X > 0) { pos.X = 0; }
                if (pos.Y > 0) { pos.Y = 0; }

                // subtract the part of the DragRemainder that we "used".
                // what's left can be "used" by someone further up the hierarchy
                input.DragRemainder -= (pos - state.OrigPos) * (1.0f / scale);
                ++input.DragRemainderUses;

                state.Pos = pos;

                scrollArea.OnPassFinished(HandleDragOverflow);
            } else if (input.IsDragging &&
                       scrollArea.AbsoluteRect.Contains(input.TrueDragOrigin) &&
                       scrollArea.ClipRect.Contains(input.TrueDragOrigin)) {
                state.IsDragging = true;
                state.OrigPos = state.Pos;
            } else {
                state.Overflow -= state.Overflow * (float)(input.TimeDelta * 20.0);
            }
        }

        private static void HandleDragOverflow(Element scrollArea)
        {
            var input = scrollArea.Context.Input;
            var state = scrollArea.GetOrCreateState<ScrollAreaState>();
            var scale = scrollArea.ToLocalScale(1.0f);
            var vec = (input.DragRemainder * scale) * (1.0f / input.DragRemainderUses);
            state.Overflow = vec * (1.0f / (float)Math.Max(Math.Sqrt(vec.Length), 1.0));
            if ((state.Flags & ScrollAreaFlags.OverDragX) == 0) {
                state.Overflow.X = 0;
            }
            if ((state.Flags & ScrollAreaFlags.OverDragY) == 0) {
                state.Overflow.Y = 0;
            }
        }

        private static void DrawOverlay(DrawContext dc)
        {
            var overlay = dc.Target;
            var scrollArea = overlay.Parent;
            var content = GetContentPanel(scrollArea);

            if (content.Width > scrollArea.Width) {
                var length = scrollArea.Width * scrollArea.Width / content.Width;
                var offset = (scrollArea.Width - length) * -content.X / (content.Width - scrollArea.Width);
                dc.SolidRect(new Rect(offset, scrollArea.Height - 5, length, 5), new Color(0, 0, 0, 64));
            }
            
            if (content.Height > scrollArea.Height) {
                var length = scrollArea.Height * scrollArea.Height / content.Height;
                var offset = (scrollArea.Height - length) * -content.Y / (content.Height - scrollArea.Height);
                dc.SolidRect(new Rect(scrollArea.Width - 5, offset, 5, length), new Color(0, 0, 0, 64));
            }
        }
    }
}
