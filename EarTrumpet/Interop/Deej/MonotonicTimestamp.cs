using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Deej
{
    public struct MonotonicTimestamp
    {
        private static double tickFrequency;

        private long timestamp;

        static MonotonicTimestamp()
        {
            NativeMethods.QueryPerformanceFrequency(out var frequency);

            tickFrequency = (double)TimeSpan.TicksPerSecond / frequency;
        }

        private MonotonicTimestamp(long timestamp)
        {
            this.timestamp = timestamp;
        }

        public static MonotonicTimestamp Now()
        {
            NativeMethods.QueryPerformanceCounter(out var value);
            return new MonotonicTimestamp(value);
        }

        public static TimeSpan operator -(MonotonicTimestamp to, MonotonicTimestamp from)
        {
            var ticks = (long)((to.timestamp - from.timestamp) * tickFrequency);
            return new TimeSpan(ticks);
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern bool QueryPerformanceCounter(out long value);

            [DllImport("kernel32.dll")]
            public static extern bool QueryPerformanceFrequency(out long value);
        }
    }
}