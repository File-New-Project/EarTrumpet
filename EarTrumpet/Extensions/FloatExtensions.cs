using System;

namespace EarTrumpet.Extensions
{
    public static class FloatExtensions
    {
        public static Int32 ToVolumeInt(this float val)
        {
            return Convert.ToInt32(Math.Round((val * 100), MidpointRounding.AwayFromZero));
        }

        public static float Bound(this float val, float min, float max)
        {
            return Math.Max(min, Math.Min(max, val));
        }
    }
}
