using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using Xunit;

namespace EarTrumpet.Tests;

public class VolumeMathTests
{
    [Theory]
    [InlineData(0, 5, 5)]
    [InlineData(3, 5, 5)]
    [InlineData(5, 5, 10)]
    [InlineData(98, 5, 100)]
    [InlineData(100, 5, 100)]
    public void NextSnapPoint_LandsOnGridAboveAndNeverExceeds100(double current, int step, double expected)
        => Assert.Equal(expected, VolumeStepper.NextSnapPoint(current, step));

    [Theory]
    [InlineData(100, 5, 95)]
    [InlineData(7, 5, 5)]
    [InlineData(5, 5, 0)]
    [InlineData(2, 5, 0)]
    [InlineData(0, 5, 0)]
    public void PreviousSnapPoint_LandsOnGridBelowAndNeverGoesNegative(double current, int step, double expected)
        => Assert.Equal(expected, VolumeStepper.PreviousSnapPoint(current, step));

    [Theory]
    [InlineData(12, 5, 10)]
    [InlineData(13, 5, 15)]
    [InlineData(99.6, 3, 100)] // 100 stays reachable when it is not on the grid
    [InlineData(1, 3, 0)]
    public void NearestSnapPoint_RoundsToGridButKeepsFullVolumeReachable(double current, int step, double expected)
        => Assert.Equal(expected, VolumeStepper.NearestSnapPoint(current, step));

    [Fact]
    public void Bound_ClampsBothEnds()
    {
        Assert.Equal(0d, (-5d).Bound(0, 1));
        Assert.Equal(1d, 5d.Bound(0, 1));
        Assert.Equal(0.5d, 0.5d.Bound(0, 1));
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(0.5)]
    [InlineData(0.01)]
    public void LogarithmicConversion_RoundTrips(double linear)
        => Assert.Equal(linear, linear.LinearToLog().LogToLinear(), 9);

    [Fact]
    public void FullVolumeIs0Db()
        => Assert.Equal(0d, 1d.LinearToLog(), 9);

    [Fact]
    public void ForceOpaque_DisablesTransparencyRegardlessOfWindows()
    {
        try
        {
            SystemSettings.ForceOpaque = true;
            Assert.False(SystemSettings.IsTransparencyEnabled);
        }
        finally
        {
            SystemSettings.ForceOpaque = false;
        }
    }
}
