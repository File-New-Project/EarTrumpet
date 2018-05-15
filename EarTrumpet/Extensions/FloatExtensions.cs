using System;

namespace EarTrumpet.Extensions
{
    static class FloatExtensions
    {
        public static Int32 ToVolumeInt(this float val)
        {
            return Convert.ToInt32(Math.Round((val * 100), MidpointRounding.AwayFromZero));
        }
    }
}
