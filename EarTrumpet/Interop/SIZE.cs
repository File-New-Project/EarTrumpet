using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        public int cx;
        public int cy;
    }
}
