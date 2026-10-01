using System;

namespace EarTrumpet.UI.Helpers;

/// <summary>
/// Single source of truth for how far one volume nudge moves and where it lands, so that the
/// tray icon, the global mouse wheel hook, the flyout sliders and the volume hotkeys all agree
/// with the step size the user configured.
/// </summary>
/// <remarks>
/// Values are a percentage (0-100). Logarithmic volume works in decibels and has its own fixed
/// step, so the user's step and snapping settings do not apply to it; callers pass the step that
/// mode should use and bound the result themselves.
/// </remarks>
public static class VolumeStepper
{
    /// <summary>Step used by scroll wheels while logarithmic volume is on.</summary>
    public const double LogarithmicWheelStep = 0.2;

    /// <summary>Step used by the volume hotkeys while logarithmic volume is on.</summary>
    public const double LogarithmicHotkeyStep = 2.0;

    /// <summary>
    /// The volume one nudge away from <paramref name="current"/>, honouring the configured step
    /// size and snap-to-grid behaviour.
    /// </summary>
    /// <param name="current">Current volume, as a percentage (or decibels while logarithmic).</param>
    /// <param name="direction">Positive to raise, negative to lower.</param>
    /// <param name="logarithmicStep">Step to use while logarithmic volume is on.</param>
    public static double Step(double current, int direction, double logarithmicStep = LogarithmicWheelStep)
    {
        var sign = Math.Sign(direction);
        if (sign == 0)
        {
            return current;
        }

        if (App.Settings.UseLogarithmicVolume)
        {
            // Decibels: the caller owns the bounds, which differ per slider.
            return current + sign * logarithmicStep;
        }

        var step = App.Settings.VolumeStepAmount;
        if (App.Settings.UseRangeSnapping)
        {
            return sign > 0 ? NextSnapPoint(current, step) : PreviousSnapPoint(current, step);
        }

        return Bound(current + sign * step);
    }

    /// <summary>First point on the step grid above <paramref name="current"/>.</summary>
    public static double NextSnapPoint(double current, int step)
    {
        return Math.Min(100, Math.Floor(current / step) * step + step);
    }

    /// <summary>First point on the step grid below <paramref name="current"/>.</summary>
    public static double PreviousSnapPoint(double current, int step)
    {
        return Math.Max(0, Math.Ceiling(current / step) * step - step);
    }

    /// <summary>
    /// Closest point on the step grid. 100 stays reachable even when it does not sit on the grid,
    /// so that a step size like 3 can still reach full volume.
    /// </summary>
    public static double NearestSnapPoint(double current, int step)
    {
        var snap = Math.Round(current / step) * step;
        return Math.Abs(100 - current) < Math.Abs(snap - current) ? 100 : Bound(snap);
    }

    private static double Bound(double value)
    {
        return Math.Max(0, Math.Min(100, value));
    }
}
