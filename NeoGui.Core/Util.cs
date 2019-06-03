
using System;

namespace NeoGui.Core
{
    public static class Util
    {
        public static long TwoIntsToLong(int a, int b) => unchecked(((long)a << 32) | (uint)b);

        public static float Clamp(float val, float min, float max) =>
            val < min ? min : (val > max ? max : val);
        

        public static double NormalizeInInterval(double t, double start, double end) =>
            Math.Min(1, Math.Max(0, t - start) / (end - start));
        
        public static double Sigmoid(double t) => 1.0 / (1.0 + Math.Exp(-t));
    }
}
