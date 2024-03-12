namespace NeoGui.Toolkit;

using System;
using NeoGui.Core;

public class ToggleSwitchState {
    public float Pos, Start, Target;
    public double T0, T1;
}

public static class ToggleSwitch {
    public static Element CreateToggleSwitch(this Element parent, bool on = false, Action<Element>? onToggled = null) {
        var toggleSwitch = parent.CreateElement();
        toggleSwitch.Name = "ToggleSwitch";
        toggleSwitch.AddButtonBehavior();
        toggleSwitch.Size = new Vec2(36, 16);
        toggleSwitch.OnDraw(Draw);
        toggleSwitch.OnInserted(OnInserted);
        toggleSwitch.OnDepthDescent(OnDepthDescent);
        if (onToggled != null) {
            toggleSwitch.Set(new ButtonCallback { OnClick = onToggled });
        }
        var state = toggleSwitch.GetOrCreateState<ToggleSwitchState>();
        var target = on ? 1f : 0f;
        if (Math.Abs(target - state.Target) > 0.000001f) {
            state.Start = state.Pos;
            state.Target = target;
            state.T0 = toggleSwitch.Context.Input.Time;
            state.T1 = state.T0 + Math.Abs(state.Target - state.Pos) * 0.15;
        }
        return toggleSwitch;
    }

    public static void OnInserted(Element toggleSwitch) {
        var state = toggleSwitch.GetOrCreateState<ToggleSwitchState>();
        state.Pos = state.Target;
    }

    private static void OnDepthDescent(Element toggleSwitch) {
        var state = toggleSwitch.GetOrCreateState<ToggleSwitchState>();
        if (Math.Abs(state.Target - state.Pos) > 0.000001f) {
            var t = Util.NormalizeInInterval(toggleSwitch.Context.Input.Time, state.T0, state.T1);
            state.Pos = state.Start + (state.Target - state.Start) * (float)t;
        }
    }

    public static void Draw(DrawContext dc) {
        var toggleSwitch = dc.Target;
        var state = toggleSwitch.GetOrCreateState<ToggleSwitchState>();
        var size = toggleSwitch.Size;
        dc.SolidRect(new Rect(size), Color.LightGray);
        TextButton.DrawButtonRect(dc, toggleSwitch, new Rect(state.Pos * (size.X - size.Y), 0, size.Y, size.Y));
    }
}
