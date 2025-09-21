using System;

namespace EarTrumpet.Extensions;

public static class FloatExtensions
{
    public static int ToVolumeInt(this float val)
    {
        return Convert.ToInt32(Math.Round(val * 100, MidpointRounding.AwayFromZero));
    }

    public static float Bound(this float val, float min, float max)
    {
        return Math.Max(min, Math.Min(max, val));
    }

    public static float LinearToLog(this float val) => (float)(20 * Math.Log10(val));

    public static float LinearToLogNormalized(this float val) =>
        val == 0
            ? 0
            : ((float)(20 * Math.Log10(val) / -App.Settings.LogarithmicVolumeMinDb + 1))
                .Bound(0, 1f);

    public static float LogToLinear(this float val) => (float)Math.Pow(10, val / 20);
}
