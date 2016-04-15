using System;
using NeoGui.Core;

namespace NeoGui.Toolkit
{
    public struct ToogleSwitchOn
    {
        public bool Value;
    }

    public class ToogleSwitchState
    {
        public bool Inited;
        public float Pos;
    }

    public static class ToggleSwitch
    {
        public static Element Create(Element parent, bool on = false, Action<Element> onToggled = null)
        {
            var toggleSwitch = Element.Create(parent);
            ButtonBehavior.Add(toggleSwitch);
            toggleSwitch.Draw = Draw;
            if (onToggled != null) {
                toggleSwitch.Set(new ButtonCallback { OnClick = onToggled });
            }
            toggleSwitch.Set(new ToogleSwitchOn { Value = on });
            toggleSwitch.OnDepthDescent(OnDepthDescent);
            toggleSwitch.Rect = new Rect(36, 16);
            return toggleSwitch;
        }

        private static void OnDepthDescent(Element toggleSwitch)
        {
            var input = toggleSwitch.Context.Input;
            var on = toggleSwitch.Get<ToogleSwitchOn>().Value;
            var state = toggleSwitch.GetOrCreateState<ToogleSwitchState>();
            if (!state.Inited) {
                state.Pos = on ? 1.0f : 0.0f;
                state.Inited = true;
            }
            var target = on ? 1.0f : 0.0f;
            state.Pos += (target - state.Pos) * (float)(input.TimeDelta * 20.0);
        }

        public static void Draw(DrawContext dc)
        {
            var toggleSwitch = dc.Target;
            var state = toggleSwitch.GetOrCreateState<ToogleSwitchState>();
            var size = toggleSwitch.Size;
            dc.SolidRect(new Rect(size), Color.Gray);
            dc.SolidRect(new Rect(state.Pos * (size.X - size.Y), 0, size.Y, size.Y), Color.DarkGray);
        }
    }
}
