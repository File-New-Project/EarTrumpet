using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public override string ToString() => $"[Left={Left},Top={Top},Right={Right},Bottom={Bottom}]";
        public bool Contains(System.Drawing.Point pt) => pt.X >= Left && pt.X <= Right && pt.Y >= Top && pt.Y <= Bottom;
    }
}
