using System;
using System.Collections.Generic;
using System.Diagnostics;
using NeoGui.Core;

namespace NeoGui
{
    public struct ButtonCallback
    {
        public Action<Element> OnClicked;
    }

    public class ButtonBehaviorState
    {
        public bool MousePressed;
    }

    public static class ButtonBehaviorExt
    {
        public static void AddButtonBehavior(this Element elem)
        {
            elem.OnDepthDescent(OnDepthDescent);
        }

        private static void OnDepthDescent(Element e)
        {
            var input = e.Context.Input;
            var state = e.GetState<ButtonBehaviorState>(true);
            if (state.MousePressed) {
                if (input.WasMouseButtonReleased(MouseButton.Left)) {
                    if (e.IntersectsMouse) {
                        var data = e.Get<ButtonCallback>();
                        data.OnClicked?.Invoke(e);
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
    


    public struct TextButtonData
    {
        public string Text;
    }

    public static class TextButtonExt
    {
        public static Element CreateTextButton(this Element parent, string text)
        {
            var elem = parent.CreateElement();
            elem.AddButtonBehavior();
            elem.Set(new TextButtonData { Text = text });
            elem.Draw = DrawTextButton;
            return elem;
        }

        public static void DrawTextButton(DrawContext dc)
        {
            var data = dc.Target.Get<TextButtonData>();
            var size = dc.Target.Size;
            var textSize = dc.TextSize(data.Text);
            var color = Color.Gray;
            if (dc.Target.IntersectsMouse) {
                var state = dc.Target.GetState<ButtonBehaviorState>();
                if (state != null) {
                    color = state.MousePressed ? Color.Black : Color.DarkGray;
                }
            }
            dc.SolidRect(new Rect(size), color);
            dc.Text((size - textSize) * 0.5f, data.Text, Color.White);
        }
    }
    


    public struct LabelText
    {
        public string Text;
    }
    public struct LabelColor
    {
        public Color Color;
    }

    public static class LabelExt
    {
        public static Element CreateLabel(this Element parent, string text)
        {
            var elem = parent.CreateElement();
            elem.Set(new LabelText { Text = text });
            elem.Set(new LabelColor { Color = Color.White });
            elem.Draw = DrawLabel;
            return elem;
        }

        public static void DrawLabel(DrawContext dc)
        {
            var text = dc.Target.Get<LabelText>().Text;
            var color = dc.Target.Get<LabelColor>().Color;
            var size = dc.Target.Size;
            var textSize = dc.TextSize(text);
            dc.Text((size - textSize) * 0.5f, text, color);
        }
    }




    public static class TestUi
    {
        private class TestState
        {
            public int ActiveTab;
            public int NumButtons = 3;
            public readonly Dictionary<ElementId, int> Counts = new Dictionary<ElementId, int>();
        }

        private static bool panelVisible = true;

        public static void DoUi(NeoGuiContext ui, float windowWidth, float windowHeight)
        {
            ui.BeginFrame();

            var root = ui.Root;
            root.Rect = new Rect(0, 0, windowWidth, windowHeight);
            root.Draw = dc => {
                dc.SolidRect(new Rect(dc.Target.Size), Color.White);
            };

            var toggleButton = root.CreateTextButton("Toggle");
            toggleButton.Rect = new Rect(70, 40, 100, 30);
            toggleButton.Set(new ButtonCallback {
                OnClicked = e => {
                    panelVisible = !panelVisible;
                }
            });

            if (panelVisible) {
                var panel = root.CreateElement();
                panel.AttachStateHolder();
                panel.Rect = new Rect(70, 80, 300, 600);
                panel.ClipContent = true;
                panel.Draw = dc => {
                    dc.SolidRect(new Rect(dc.Target.Size), Color.LightGray);
                };
                var state = panel.GetState<TestState>(true);

                var tabButton0 = panel.CreateTextButton("Tab 0");
                tabButton0.Rect = new Rect(0, 0, 100, 30);
                tabButton0.Set(new ButtonCallback {
                    OnClicked = e => {
                        var s = e.Parent.GetState<TestState>();
                        s.ActiveTab = 0;
                    }
                });

                var tabButton1 = panel.CreateTextButton("Tab 1");
                tabButton1.Rect = new Rect(101, 0, 100, 30);
                tabButton1.Set(new ButtonCallback {
                    OnClicked = e => {
                        var s = e.Parent.GetState<TestState>();
                        s.ActiveTab = 1;
                    }
                });

                if (state.ActiveTab == 0) {
                    var tab0 = panel.CreateElement();
                    tab0.Rect = new Rect(0, 30, 300, 550);

                    var label = tab0.CreateLabel("This is tab 0");
                    label.Set(new LabelColor { Color = Color.Black });
                    label.Rect = new Rect(10, 10, 100, 30);

                    var addButton = tab0.CreateTextButton("Add");
                    addButton.Rect = new Rect(150, 10, 100, 30);
                    addButton.Set(new ButtonCallback {
                        OnClicked = e => {
                            var s = e.Parent.Parent.GetState<TestState>();
                            ++s.NumButtons;
                        }
                    });

                    for (var i = 0; i < state.NumButtons; ++i) {
                        var button = tab0.CreateTextButton("Ok");
                        button.Rect = new Rect(10, 50 + i * 40, 100 + (float)Math.Sin(ui.Input.Time * 3 + i * 0.1f) * 30, 30);
                        button.Set(new ButtonCallback {
                            OnClicked = e => {
                                var s = e.Parent.Parent.GetState<TestState>();
                                int cnt;
                                state.Counts.TryGetValue(button.Id, out cnt);
                                s.Counts[e.Id] = cnt + 1;
                            }
                        });

                        int count;
                        state.Counts.TryGetValue(button.Id, out count);
                        var countLabel = tab0.CreateLabel($"count: {count}");
                        countLabel.Set(new LabelColor { Color = Color.Black });
                        countLabel.Rect = new Rect(170, 50 + i * 40, 100, 30);
                    }
                } else {
                    var tab1 = panel.CreateElement();
                    tab1.Rect = new Rect(0, 30, 300, 550);

                    var label = tab1.CreateLabel("This is tab 1");
                    label.Set(new LabelColor { Color = Color.Black });
                    label.Rect = new Rect(10, 10, 100, 30);
                }
            }

            ui.EndFrame();
        }
    }
}