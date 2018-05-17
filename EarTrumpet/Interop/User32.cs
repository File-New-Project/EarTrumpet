using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EarTrumpet.Interop
{
    public class User32
    {
        public const int WM_USERMAGIC = 1120;
        public const int SNDVOL_ACTION_SHOWCONTEXTMENU = 123;

        public static uint MAKEWPARAM(ushort low, ushort high)
        {
            return ((uint)high << 16) | low;
        }

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(
            IntPtr hwnd,
            ref WindowCompositionAttribData data);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern IntPtr FindWindowW(
            [MarshalAs(UnmanagedType.LPWStr)]string lpClassName,
            string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(
            IntPtr hwnd,
            out Win32.RECT lpRect);

        [DllImport("user32.dll", PreserveSig = true)]
        internal static extern int IsImmersiveProcess(
            IntPtr hProcess);

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern bool DestroyIcon(
            IntPtr iconHandle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern int GetClassName(
            IntPtr hWnd,
            StringBuilder strText,
            int maxCount);

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern uint GetWindowThreadProcessId(
            IntPtr hWnd,
            out uint lpdwProcessId);

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern bool EnumWindows(
            EnumWindowsProc enumProc,
            IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern IntPtr SendMessage(
            IntPtr hWnd,
            int wMsg,
            IntPtr wParam,
            IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttribData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public AccentFlags AccentFlags;
            public uint GradientColor;
            public uint AnimationId;
        }

        [Flags]
        internal enum AccentFlags
        {
            // ...
            DrawLeftBorder = 0x20,
            DrawTopBorder = 0x40,
            DrawRightBorder = 0x80,
            DrawBottomBorder = 0x100,
            DrawAllBorders = (DrawLeftBorder | DrawTopBorder | DrawRightBorder | DrawBottomBorder)
            // ...
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_INVALID_STATE = 5
        }

        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 SWP_NOZORDER = 0x0004;
    }
}
