using System;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet.Tests;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        var failures = 0;
        var cases = FocusedAppAudioControlTests.Cases()
            .Concat(ForegroundAppResolverTests.Cases())
            .Concat(AudioDeviceSessionVolumeTests.Cases()).ToArray();
        foreach (var (name, test) in cases)
        {
            try
            {
                test();
                Console.WriteLine($"PASS {name}");
            }
            catch (Exception ex)
            {
                failures++;
                Console.Error.WriteLine($"FAIL {name}: {ex}");
            }
        }

        Console.WriteLine($"{cases.Length - failures}/{cases.Length} tests passed.");
        return failures == 0 ? 0 : 1;
    }
}

internal static class Check
{
    internal static void True(bool condition)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Expected true.");
        }
    }

    internal static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected {expected}, got {actual}.");
        }
    }

    internal static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        if (!expected.SequenceEqual(actual))
        {
            throw new InvalidOperationException($"Expected [{string.Join(", ", expected)}], got [{string.Join(", ", actual)}].");
        }
    }
}
