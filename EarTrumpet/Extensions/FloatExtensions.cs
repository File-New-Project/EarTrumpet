using System;

namespace EarTrumpet.Extensions;

public static class FloatExtensions
{
    public static int ToVolumeInt(this float val) => ToVolumeInt((double)val);
    public static int ToVolumeInt(this double val) => Convert.ToInt32(Math.Round(val * 100, MidpointRounding.AwayFromZero));

    public static float Bound(this float val, float min, float max) => (float)Bound((double)val, min, max);
    public static double Bound(this double val, double min, double max) => Math.Max(min, Math.Min(max, val));

    public static float LinearToLog(this float val) => (float)LinearToLog((double)val);
    public static double LinearToLog(this double val) => (double)(20 * Math.Log10(val));

    public static float LinearToLogNormalized(this float val) => (float)LinearToLogNormalized((double)val);
    public static double LinearToLogNormalized(this double val) =>
        val == 0
            ? 0
            : ((double)(20 * Math.Log10(val) / -App.Settings.LogarithmicVolumeMinDb + 1))
                .Bound(0, 1f);

    public static float LogToLinear(this float val) => (float)LogToLinear((double)val);
    public static double LogToLinear(this double val) => (double)Math.Pow(10, val / 20);
}
