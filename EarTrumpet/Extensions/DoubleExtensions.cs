using System;

namespace EarTrumpet.Extensions
{
    static class DoubleExtensions
    {
        public static double Bound(this double val, double min, double max)
        {
            return Math.Max(min, Math.Min(max, val));
        }
    }
}
