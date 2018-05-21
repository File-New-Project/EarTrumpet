using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    public static class Comctl32
    {
        [DllImport("comctl32.dll", PreserveSig = false)]
        internal static extern void LoadIconMetric(
            IntPtr instanceHandle,
            IntPtr iconId,
            LI_METRIC desiredMetric,
            ref IntPtr icon);

        internal enum LI_METRIC
        {
            LIM_SMALL = 0,
            LIM_LARGE = 1,
        }
    }
}
