namespace NeoGui.Toolkit;

using System;
using System.Diagnostics;
using NeoGui.Core;

public class ButtonState {
    public bool MousePressed;
    public bool EnterPressed;
    public bool SpacePressed;
}

public struct ButtonCallback {
    public Action<Element> OnClick;
}

public static class ButtonBehavior {
    private static readonly ButtonCallback DefaultCallback = new() { OnClick = e => { } };

    public static Element AddButtonBehavior(this Element e, Action<Element>? onClick = null) {
        e.OnDepthDescent(OnDepthDescent);
        e.OnTreeDescent(OnTreeDescent);
        e.OnRemoved(OnRemoved);
        if (onClick != null) {
            e.Set(new ButtonCallback { OnClick = onClick });
        }
        return e;
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
                input.ConsumeMouseButtonPressed(MouseButton.Left);
                state.MousePressed = true;
            }
        }
    }

    private static void OnTreeDescent(Element e) {
        var input = e.Context.Input;
        if (e.Context.FocusId == 0) {
            if (!e.Disabled) {
                e.Context.FocusId = e.Id;
                Debug.WriteLine($"Grab focus: {e.Id} '{GetLabel(e)}'");
            }
        } else if (e.HasFocus) {
            var state = e.GetOrCreateState<ButtonState>();
            if (e.Disabled) {
                e.Context.FocusId = 0;
                state.EnterPressed = false;
                state.SpacePressed = false;
                Debug.WriteLine($"Release focus (disabled): {e.Id} '{GetLabel(e)}'");
            } else if (input.WasKeyPressed(KeyboardKey.Tab)) {
                input.ConsumeKeyPressed(KeyboardKey.Tab);
                e.Context.FocusId = 0;
                Debug.WriteLine($"Release focus (tab): {e.Id} '{GetLabel(e)}'");
            } else if (input.WasKeyPressed(KeyboardKey.Enter)) {
                input.ConsumeKeyPressed(KeyboardKey.Enter);
                state.EnterPressed = true;
            } else if (input.WasKeyPressed(KeyboardKey.Space)) {
                input.ConsumeKeyPressed(KeyboardKey.Space);
                state.SpacePressed = true;
            } else if (state.EnterPressed && input.WasKeyReleased(KeyboardKey.Enter)) {
                state.EnterPressed = false;
                state.SpacePressed = false;
                e.Get(DefaultCallback).OnClick(e);
            } else if (state.SpacePressed && input.WasKeyReleased(KeyboardKey.Space)) {
                state.EnterPressed = false;
                state.SpacePressed = false;
                e.Get(DefaultCallback).OnClick(e);
            }
        }
    }

    private static string GetLabel(Element e) {
        string label = Label.GetText(e);
        if (string.IsNullOrEmpty(label)) {
            label = e.Name;
        }
        return label;
    }

    private static void OnRemoved(ElementStateProxy e) {
        if (e.HasFocus) {
            e.Context.FocusId = 0;
            Debug.WriteLine($"Release focus (removed): {e.Id}");
        }
    }
}


public static class TextButton {
    private static readonly ButtonState DefaultState = new();

    public static Element CreateTextButton(this Element parent, string text, Action<Element> onClick) {
        return parent.CreateLabel(text, Color.White, TextAlignment.Center)
            .AddButtonBehavior(onClick)
            .SetSizeToFit(false)
            .OnDraw(Draw);
    }

    public static void Draw(DrawContext dc) {
        var button = dc.Target;
        var size = button.Size;
        DrawButtonRect(dc, button, new Rect(size));
        Label.Draw(dc);
        if (button.Disabled) {
            dc.SolidRect(new Rect(size), new Color(0, 0, 0, 64));
        }
    }

    public static void DrawButtonRect(DrawContext dc, Element button, Rect rect) {
        var color = Color.Gray;
        if (!button.Disabled) {
            var state = button.GetState(DefaultState);
            bool focused = button.HasFocus;
            bool hovered = button.IsUnderMouse;
            bool down = focused && (state.EnterPressed || state.SpacePressed) || hovered && state.MousePressed;
            if (down) {
                color = Color.Black;
            } else if (hovered && focused) {
                color = Color.DarkBlue;
            } else if (hovered) {
                color = Color.DarkGray;
            } else if (focused) {
                color = Color.Blue;
            }
        }
        dc.SolidRect(rect, color);
    }
}
