
using System;

namespace NeoGui.Core
{
    internal static class Util
    {
        public static long TwoIntsToLong(int a, int b)
        {
            unchecked {
                return ((long)a << 32) | (uint)b;
            }
        }

        public static float Clamp(float val, float min, float max)
        {
            return val < min ? min : (val > max ? max : val);
        }
    }
}
