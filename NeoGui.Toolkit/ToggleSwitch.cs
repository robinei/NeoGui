namespace NeoGui.Toolkit;

using System;
using NeoGui.Core;

public static class ToggleSwitch {
    private struct ToggleSwitchValue {
        public bool On;
    }

    public static Element CreateToggleSwitch(this Element parent, bool on = false, Action<Element>? onToggled = null) {
        return parent.CreateElement()
            .SetName("ToggleSwitch")
            .AddButtonBehavior(onToggled)
            .SetSize(36, 16)
            .OnDraw(Draw)
            .Set(new ToggleSwitchValue { On = on });
    }

    public static void Draw(DrawContext dc) {
        Element toggleSwitch = dc.Target;
        Vec2 size = toggleSwitch.Size;
        dc.SolidRect(new Rect(size), Color.LightGray);
        bool on = toggleSwitch.Get<ToggleSwitchValue>().On;
        float x = toggleSwitch.Animate<ToggleSwitchValue>(on ? 1.0f : 0.0f) * (size.X - size.Y);
        TextButton.DrawButtonRect(dc, toggleSwitch, new Rect(x, 0, size.Y, size.Y));
    }
}
