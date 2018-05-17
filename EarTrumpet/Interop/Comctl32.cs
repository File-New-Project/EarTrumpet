using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    public static class Comctl32
    {
        [DllImport("comctl32.dll", PreserveSig = false)]
        public static extern void LoadIconMetric(
            IntPtr instanceHandle,
            IntPtr iconId,
            LoadIconDesiredMetric desiredMetric,
            ref IntPtr icon);

        public enum LoadIconDesiredMetric
        {
            Small,
            Large,
        }
    }
}
