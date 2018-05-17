using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    public static class Win32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
