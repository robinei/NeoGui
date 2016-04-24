using System;
using NeoGui.Core;

namespace NeoGui.Toolkit
{
    public class ToogleSwitchState
    {
        public float Pos, Start, Target;
        public double T0, T1;
    }

    public static class ToggleSwitch
    {
        public static Element Create(Element parent, bool on = false, Action<Element> onToggled = null)
        {
            var toggleSwitch = Element.Create(parent);
            ButtonBehavior.Add(toggleSwitch);
            toggleSwitch.Size = new Vec2(36, 16);
            toggleSwitch.Draw = Draw;
            toggleSwitch.OnInserted(OnInserted);
            toggleSwitch.OnDepthDescent(OnDepthDescent);
            if (onToggled != null) {
                toggleSwitch.Set(new ButtonCallback { OnClick = onToggled });
            }
            var state = toggleSwitch.GetOrCreateState<ToogleSwitchState>();
            var target = on ? 1f : 0f;
            if (Math.Abs(target - state.Target) > 0.000001f) {
                state.Start = state.Pos;
                state.Target = target;
                state.T0 = toggleSwitch.Context.Input.Time;
                state.T1 = state.T0 + Math.Abs(state.Target - state.Pos) * 0.15;
            }
            return toggleSwitch;
        }

        public static void OnInserted(Element toggleSwitch)
        {
            var state = toggleSwitch.GetOrCreateState<ToogleSwitchState>();
            state.Pos = state.Target;
        }

        private static void OnDepthDescent(Element toggleSwitch)
        {
            var state = toggleSwitch.GetOrCreateState<ToogleSwitchState>();
            if (Math.Abs(state.Target - state.Pos) > 0.000001f) {
                var t = Util.NormalizeInInterval(toggleSwitch.Context.Input.Time, state.T0, state.T1);
                state.Pos = state.Start + (state.Target - state.Start) * (float)t;
            }
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
