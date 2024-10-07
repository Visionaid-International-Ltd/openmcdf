using System;

namespace OpenMcdf
{
    internal static class MathEx
    {
        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
