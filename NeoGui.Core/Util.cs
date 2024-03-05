namespace NeoGui.Core;

using System;
using System.Runtime.InteropServices;

public static class Util {
    public static long TwoIntsToLong(int a, int b) => unchecked(((long)a << 32) | (uint)b);

    public static float Clamp(float val, float min, float max) =>
        val < min ? min : (val > max ? max : val);
    

    public static double NormalizeInInterval(double t, double start, double end) =>
        Math.Min(1, Math.Max(0, t - start) / (end - start));
    
    public static double Sigmoid(double t) => 1.0 / (1.0 + Math.Exp(-t));

#pragma warning disable 649
    private struct AlignmentHelper<T> where T : unmanaged {
        public byte Padding;
        public T Target;
    }
#pragma warning restore 649
    public static int AlignmentOf<T>() where T : unmanaged {
        return (int)Marshal.OffsetOf<AlignmentHelper<T>>(nameof(AlignmentHelper<T>.Target));
    }
}
