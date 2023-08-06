using System;

namespace EarTrumpet.Extensions
{
    public static class FloatExtensions
    {
        private const float CURVE_FACTOR = 5.757f;

        public static int ToVolumeInt(this float val)
        {
            return Convert.ToInt32(Math.Round(val * 100, MidpointRounding.AwayFromZero));
        }

        public static float Bound(this float val, float min, float max)
        {
            return Math.Max(min, Math.Min(max, val));
        }

        public static float ToLogVolume(this float val)
        {
            return ((float)(Math.Exp(CURVE_FACTOR * val) / Math.Exp(CURVE_FACTOR))).Bound(0, 1f);
        }

        public static float ToDisplayVolume(this float val)
        {
            return ((float)(Math.Log(val * Math.Exp(CURVE_FACTOR)) / CURVE_FACTOR)).Bound(0, 1f);
        }
    }
}
