namespace NeoGui.Toolkit;

using NeoGui.Core;
using System;

public class AnimationState<T> {
    public bool Initialized;
    public float Pos, Start, Target;
    public double T0, T1;
}

public static class Animation {

    public static float Animate<T>(this Element e, float target) {
        var state = e.GetOrCreateState<AnimationState<T>>();

        if (!state.Initialized) {
            state.Initialized = true;
            state.Pos = state.Start = state.Target = target;
        }
        
        if (Math.Abs(target - state.Target) > 0.000001f) {
            state.Start = state.Pos;
            state.Target = target;
            state.T0 = e.Context.Input.Time;
            state.T1 = state.T0 + Math.Abs(state.Target - state.Pos) * 0.15;
        }

        if (Math.Abs(state.Target - state.Pos) > 0.000001f) {
            var t = Util.NormalizeInInterval(e.Context.Input.Time, state.T0, state.T1);
            state.Pos = state.Start + (state.Target - state.Start) * (float)t;
        }

        return state.Pos;
    }
}