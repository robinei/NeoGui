using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Policy;
using NeoGui.Core;

namespace NeoGui
{
    public struct LabelText { public string Value; }
    public struct LabelColor { public Color Value; }

    public static class Label
    {
        private static readonly LabelText DefaultText = new LabelText {Value = ""};
        private static readonly LabelColor DefaultColor = new LabelColor {Value = Color.White};

        public static Element Create(Element parent, string text, Color? color = null)
        {
            var label = Element.Create(parent);
            if (text != null) {
                label.Set(new LabelText { Value = text });
            }
            if (color != null) {
                label.Set(new LabelColor { Value = color.Value });
            }
            label.Draw = Draw;
            return label;
        }

        public static void Draw(DrawContext dc)
        {
            var label = dc.Target;
            var text = label.Get(DefaultText).Value;
            var color = label.Get(DefaultColor).Value;
            var size = label.Size;
            var textSize = dc.TextSize(text);
            dc.Text((size - textSize) * 0.5f, text, color);
        }
    }




    public class ButtonState
    {
        public bool MousePressed;
    }

    public struct ButtonCallback { public Action<Element> OnClick; }

    public static class ButtonBehavior
    {
        public static void Add(Element elem)
        {
            elem.OnDepthDescent(OnDepthDescent);
        }

        private static void OnDepthDescent(Element e)
        {
            var input = e.Context.Input;
            var state = e.GetOrCreateState<ButtonState>();
            if (!e.Enabled) {
                state.MousePressed = false;
                return;
            }
            if (state.MousePressed) {
                if (input.WasMouseButtonReleased(MouseButton.Left)) {
                    if (e.IsUnderMouse) {
                        e.Get<ButtonCallback>().OnClick?.Invoke(e);
                    }
                    state.MousePressed = false;
                }
            } else {
                if (input.WasMouseButtonPressed(MouseButton.Left) && e.IsUnderMouse) {
                    state.MousePressed = true;
                    input.ConsumeMouseButtonPressed(MouseButton.Left);
                }
            }
        }
    }
    
    

    public static class TextButton
    {
        public static Element Create(Element parent, string text, Action<Element> onClick = null)
        {
            var button = Label.Create(parent, text);
            ButtonBehavior.Add(button);
            button.Draw = Draw;
            if (onClick != null) {
                button.Set(new ButtonCallback { OnClick = onClick });
            }
            return button;
        }

        public static void Draw(DrawContext dc)
        {
            var button = dc.Target;
            var size = button.Size;
            var color = Color.Gray;
            if (button.Enabled && button.IsUnderMouse) {
                var state = button.GetState<ButtonState>();
                if (state != null) {
                    color = state.MousePressed ? Color.Black : Color.DarkGray;
                }
            }
            dc.SolidRect(new Rect(size), color);
            Label.Draw(dc);
            if (!button.Enabled) {
                dc.SolidRect(new Rect(size), new Color(0, 0, 0, 64));
            }
        }
    }
    


    public struct BackgroundColor { public Color Value; }

    public static class DrawFuncs
    {
        private static readonly BackgroundColor DefaultBackgroundColor = new BackgroundColor {Value = Color.White};

        public static void DrawBackgroundColor(DrawContext dc)
        {
            var size = dc.Target.Size;
            var color = dc.Target.Get(DefaultBackgroundColor).Value;
            dc.SolidRect(new Rect(size), color);
        }
    }



    public class ScrollAreaState
    {
        public Vec2 Overflow;
        public Vec2 Pos;
        public Vec2 OrigPos;
        public bool IsDragging;
    }

    public static class ScrollArea
    {
        public static Element Create(Element parent, object key = null)
        {
            var scrollArea = Element.Create(parent, key);
            scrollArea.Name = "ScrollArea";
            scrollArea.ClipContent = true;
            scrollArea.Layout = Layout;
            scrollArea.OnDepthDescent(OnDepthDescent);

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

        private static void Layout(Element scrollArea)
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
                var len = state.Overflow.Length;
                if (len > 0.1) {
                    state.Overflow -= state.Overflow.Normalized * (float)(input.TimeDelta * len * 10);
                }
            }
        }

        private static void HandleDragOverflow(Element scrollArea)
        {
            var input = scrollArea.Context.Input;
            var state = scrollArea.GetOrCreateState<ScrollAreaState>();
            var scale = scrollArea.ToLocalScale(1.0f);
            var vec = (input.DragRemainder * scale) * (1.0f / input.DragRemainderUses);
            state.Overflow = vec * (1.0f / (float)Math.Sqrt(vec.Length));
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



    public static class TestUi
    {
        private class TestState
        {
            public int ActiveTab = 1;
            public int NumButtons = 3;
        }

        private class TestCount
        {
            public int Value;
            public string StringValue;
        }

        private static bool panelVisible = true;

        public static void DoUi(NeoGuiContext ui, float windowWidth, float windowHeight)
        {
            ui.BeginFrame();

            var root = ui.Root;
            root.Rect = new Rect(0, 0, windowWidth, windowHeight);
            root.Draw = DrawFuncs.DrawBackgroundColor;

            var toggleButton = TextButton.Create(root, "Toggle", e => {
                panelVisible = !panelVisible;
            });
            toggleButton.Rect = new Rect(70, 40, 100, 30);

            if (panelVisible) {
                var panel = Element.Create(root);
                panel.AttachStateHolder();
                panel.Rect = new Rect(70, 80, 500, 600);
                panel.ClipContent = true;
                panel.Set(new BackgroundColor {Value = Color.LightGray});
                panel.Draw = DrawFuncs.DrawBackgroundColor;
                
                var state = panel.GetOrCreateState<TestState>();

                var tabButton0 = TextButton.Create(panel, "Tab 0", e => {
                    e.FindState<TestState>().ActiveTab = 0;
                });
                tabButton0.Enabled = state.ActiveTab != 0;
                tabButton0.Rect = new Rect(0, 0, 100, 30);

                var tabButton1 = TextButton.Create(panel, "Tab 1", e => {
                    e.FindState<TestState>().ActiveTab = 1;
                });
                tabButton1.Enabled = state.ActiveTab != 1;
                tabButton1.Rect = new Rect(101, 0, 100, 30);
                
                if (state.ActiveTab == 0) {
                    var tab0 = Element.Create(panel);
                    tab0.Rect = new Rect(0, 30, 300, 550);

                    var titleLabel = Label.Create(tab0, "This is tab 0", Color.Black);
                    titleLabel.Rect = new Rect(10, 10, 100, 30);

                    var addButton = TextButton.Create(tab0, "Add", e => {
                        e.FindState<TestState>().NumButtons++;
                    });
                    addButton.Rect = new Rect(150, 10, 100, 30);

                    for (var i = 0; i < state.NumButtons; ++i) {
                        var button = TextButton.Create(tab0, "Ok", e => {
                            var s = e.GetOrCreateState<TestCount>();
                            s.StringValue = $"count: {++s.Value}";
                        });
                        button.Rect = new Rect(10, 50 + i * 40, 100 + (float)Math.Sin(ui.Input.Time * 3 + i * 0.1f) * 30, 30);

                        var countString = button.GetOrCreateState<TestCount>().StringValue;
                        var countLabel = Label.Create(tab0, countString, Color.Black);
                        countLabel.Rect = new Rect(170, 50 + i * 40, 100, 30);
                    }
                } else {
                    var tab1 = Element.Create(panel);
                    tab1.Rect = new Rect(0, 30, 300, 550);

                    var titleLabel = Label.Create(tab1, "This is tab 1", Color.Black);
                    titleLabel.Rect = new Rect(10, 10, 100, 30);
                    
                    
                    var outerScrollArea = ScrollArea.Create(tab1);
                    outerScrollArea.Rect = new Rect(10, 40, 300, 300);
                    outerScrollArea.Set(new BackgroundColor { Value = Color.LightGray });
                    outerScrollArea.Draw = DrawFuncs.DrawBackgroundColor;

                    var outerContentPanel = ScrollArea.GetContentPanel(outerScrollArea);
                    outerContentPanel.Size = new Vec2(500, 500);
                    outerContentPanel.Set(new BackgroundColor { Value = new Color(240, 240, 240) });
                    outerContentPanel.Draw = DrawFuncs.DrawBackgroundColor;


                    var scrollArea = ScrollArea.Create(outerContentPanel);
                    scrollArea.Rect = new Rect(50, 50, 200, 200);
                    scrollArea.Set(new BackgroundColor { Value = new Color(240, 240, 240) });
                    scrollArea.Draw = DrawFuncs.DrawBackgroundColor;

                    var contentPanel = ScrollArea.GetContentPanel(scrollArea);
                    contentPanel.Size = new Vec2(250, 250);
                    contentPanel.Set(new BackgroundColor { Value = new Color(220, 220, 220) });
                    contentPanel.Draw = DrawFuncs.DrawBackgroundColor;

                    var label = Label.Create(contentPanel, "Drag me", Color.Black);
                    label.Rect = new Rect(10, 10, 100, 30);

                    var button = TextButton.Create(contentPanel, "Hello", e => Debug.WriteLine("Hello"));
                    button.Rect = new Rect(10, 50, 100, 30);
                }
            }

            ui.EndFrame();
        }
    }
}