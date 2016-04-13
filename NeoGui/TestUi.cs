using System;
using System.Collections.Generic;
using NeoGui.Core;

namespace NeoGui
{
    public struct LabelText { public string Value; }
    public struct LabelColor { public Color Value; }

    public static class LabelExt
    {
        private static readonly LabelText DefaultText = new LabelText {Value = ""};
        private static readonly LabelColor DefaultColor = new LabelColor {Value = Color.White};

        public static Element CreateLabel(this Element parent, string text, Color? color = null)
        {
            var elem = parent.CreateElement();
            if (text != null) {
                elem.Set(new LabelText { Value = text });
            }
            if (color != null) {
                elem.Set(new LabelColor { Value = color.Value });
            }
            elem.Draw = DrawLabel;
            return elem;
        }

        public static void DrawLabel(DrawContext dc)
        {
            var text = dc.Target.Get(DefaultText).Value;
            var color = dc.Target.Get(DefaultColor).Value;
            var size = dc.Target.Size;
            var textSize = dc.TextSize(text);
            dc.Text((size - textSize) * 0.5f, text, color);
        }
    }




    public class ButtonState
    {
        public bool MousePressed;
    }

    public struct ButtonCallback { public Action<Element> OnClick; }

    public static class ButtonBehaviorExt
    {
        public static void AddButtonBehavior(this Element elem)
        {
            elem.OnDepthDescent(OnDepthDescent);
        }

        public static void OnDepthDescent(Element e)
        {
            var input = e.Context.Input;
            var state = e.GetOrCreateState<ButtonState>();
            if (!e.Enabled) {
                state.MousePressed = false;
                return;
            }
            if (state.MousePressed) {
                if (input.WasMouseButtonReleased(MouseButton.Left)) {
                    if (e.IntersectsMouse) {
                        e.Get<ButtonCallback>().OnClick?.Invoke(e);
                    }
                    state.MousePressed = false;
                }
            } else {
                if (input.WasMouseButtonPressed(MouseButton.Left) && e.IntersectsMouse) {
                    state.MousePressed = true;
                }
            }
        }
    }
    
    

    public static class TextButtonExt
    {
        public static Element CreateTextButton(this Element parent, string text, Action<Element> onClick = null)
        {
            var elem = parent.CreateLabel(text);
            elem.AddButtonBehavior();
            elem.Draw = DrawTextButton;
            if (onClick != null) {
                elem.Set(new ButtonCallback { OnClick = onClick });
            }
            return elem;
        }

        public static void DrawTextButton(DrawContext dc)
        {
            var elem = dc.Target;
            var size = elem.Size;
            var color = Color.Gray;
            if (elem.Enabled && elem.IntersectsMouse) {
                var state = elem.GetState<ButtonState>();
                if (state != null) {
                    color = state.MousePressed ? Color.Black : Color.DarkGray;
                }
            }
            dc.SolidRect(new Rect(size), color);
            LabelExt.DrawLabel(dc);
            if (!elem.Enabled) {
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



    public static class TestUi
    {
        private class TestState
        {
            public int ActiveTab;
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

            var toggleButton = root.CreateTextButton("Toggle", e => {
                panelVisible = !panelVisible;
            });
            toggleButton.Rect = new Rect(70, 40, 100, 30);

            if (panelVisible) {
                var panel = root.CreateElement();
                panel.AttachStateHolder();
                panel.Rect = new Rect(70, 80, 300, 600);
                panel.ClipContent = true;
                panel.Set(new BackgroundColor {Value = Color.LightGray});
                panel.Draw = DrawFuncs.DrawBackgroundColor;
                
                var state = panel.GetOrCreateState<TestState>();

                var tabButton0 = panel.CreateTextButton("Tab 0", e => {
                    e.FindState<TestState>().ActiveTab = 0;
                });
                tabButton0.Enabled = state.ActiveTab != 0;
                tabButton0.Rect = new Rect(0, 0, 100, 30);

                var tabButton1 = panel.CreateTextButton("Tab 1", e => {
                    e.FindState<TestState>().ActiveTab = 1;
                });
                tabButton1.Enabled = state.ActiveTab != 1;
                tabButton1.Rect = new Rect(101, 0, 100, 30);
                
                if (state.ActiveTab == 0) {
                    var tab0 = panel.CreateElement();
                    tab0.Rect = new Rect(0, 30, 300, 550);

                    var label = tab0.CreateLabel("This is tab 0", Color.Black);
                    label.Rect = new Rect(10, 10, 100, 30);

                    var addButton = tab0.CreateTextButton("Add", e => {
                        e.FindState<TestState>().NumButtons++;
                    });
                    addButton.Rect = new Rect(150, 10, 100, 30);

                    for (var i = 0; i < state.NumButtons; ++i) {
                        var button = tab0.CreateTextButton("Ok", e => {
                            var s = e.GetOrCreateState<TestCount>();
                            s.StringValue = $"count: {++s.Value}";
                        });
                        button.Rect = new Rect(10, 50 + i * 40, 100 + (float)Math.Sin(ui.Input.Time * 3 + i * 0.1f) * 30, 30);

                        var countString = button.GetOrCreateState<TestCount>().StringValue;
                        var countLabel = tab0.CreateLabel(countString, Color.Black);
                        countLabel.Rect = new Rect(170, 50 + i * 40, 100, 30);
                    }
                } else {
                    var tab1 = panel.CreateElement();
                    tab1.Rect = new Rect(0, 30, 300, 550);

                    var label = tab1.CreateLabel("This is tab 1", Color.Black);
                    label.Rect = new Rect(10, 10, 100, 30);
                }
            }

            ui.EndFrame();
        }
    }
}