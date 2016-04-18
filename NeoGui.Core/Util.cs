
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
    }
}
