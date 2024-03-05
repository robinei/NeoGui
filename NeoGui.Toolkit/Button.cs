namespace NeoGui.Toolkit;

using System;
using NeoGui.Core;

public class ButtonState {
    public bool MousePressed;
}

public struct ButtonCallback {
    public Action<Element> OnClick;
}

public static class ButtonBehavior {
    private static readonly ButtonCallback DefaultCallback = new() { OnClick = e => { } };

    public static void Add(Element elem) {
        elem.OnDepthDescent(OnDepthDescent);
    }

    private static void OnDepthDescent(Element e) {
        var input = e.Context.Input;
        var state = e.GetOrCreateState<ButtonState>();
        if (e.Disabled) {
            state.MousePressed = false;
            return;
        }
        if (state.MousePressed) {
            if (input.WasMouseButtonReleased(MouseButton.Left)) {
                if (e.IsUnderMouse) {
                    e.Get(DefaultCallback).OnClick(e);
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


public static class TextButton {
    private static readonly ButtonState DefaultState = new();

    public static Element Create(Element parent, string text, Action<Element>? onClick = null) {
        var button = Label.Create(parent, text, Color.White, TextAlignment.Center);
        ButtonBehavior.Add(button);
        button.SizeToFit = false;
        button.Draw = Draw;
        if (onClick != null) {
            button.Set(new ButtonCallback { OnClick = onClick });
        }
        return button;
    }

    public static void Draw(DrawContext dc) {
        var button = dc.Target;
        var size = button.Size;
        var color = Color.Gray;
        if (!button.Disabled && button.IsUnderMouse) {
            color = button.GetState(DefaultState).MousePressed ? Color.Black : Color.DarkGray;
        }
        dc.SolidRect(new Rect(size), color);
        Label.Draw(dc);
        if (button.Disabled) {
            dc.SolidRect(new Rect(size), new Color(0, 0, 0, 64));
        }
    }
}
