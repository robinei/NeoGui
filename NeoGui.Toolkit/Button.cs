using System;
using NeoGui.Core;

namespace NeoGui.Toolkit
{
    public class ButtonState
    {
        public bool MousePressed;
    }

    public struct ButtonCallback { public Action<Element> OnClick; }

    public static class ButtonBehavior
    {
        public static void Add(Element elem)
        {
            elem.OnDepthDescent(e => OnDepthDescent(e));
        }

        private static void OnDepthDescent(Element e)
        {
            var input = e.Context.Input;
            var state = e.GetOrCreateState<ButtonState>();
            if (e.Disabled) {
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
            var button = Label.Create(parent, text, Color.White, TextAlignment.Center);
            ButtonBehavior.Add(button);
            button.SizeToFit = false;
            button.Draw = dc => Draw(dc);
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
            if (!button.Disabled && button.IsUnderMouse) {
                var state = button.GetState<ButtonState>();
                if (state != null) {
                    color = state.MousePressed ? Color.Black : Color.DarkGray;
                }
            }
            dc.SolidRect(new Rect(size), color);
            Label.Draw(dc);
            if (button.Disabled) {
                dc.SolidRect(new Rect(size), new Color(0, 0, 0, 64));
            }
        }
    }
}
