using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Win32
{
    public partial class PInvoke
    {
        // Missing constant https://github.com/microsoft/win32metadata/issues/1784

        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
        public static readonly IntPtr GR_GLOBAL = new(-2);
    }
}

namespace EarTrumpet.Interop
{
    public class User32
    {
        public static uint MAKEWPARAM(ushort low, ushort high) => ((uint)high << 16) | low;

        // CsWin32 doesn't support handling cross-arch differences for [Get/Set]WindowLong[Ptr]
        // https://github.com/microsoft/CsWin32/issues/343

        public static nint GetWindowLong(HWND hwnd, WINDOW_LONG_PTR_INDEX index)
        {
#if X86
            return PInvoke.GetWindowLong(hwnd, index);
#else
            return PInvoke.GetWindowLongPtr(hwnd, index);
#endif
        }

        public static nint SetWindowLong(HWND hwnd, WINDOW_LONG_PTR_INDEX index, int value)
        {
#if X86
            return PInvoke.SetWindowLong(hwnd, index, value);
#else
            return PInvoke.SetWindowLongPtr(hwnd, index, value);
#endif
        }

        [DllImport("user32.dll", PreserveSig = true)]
        internal static extern int SetWindowCompositionAttribute(
            IntPtr hwnd,
            ref WindowCompositionAttribData data);

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
        public enum AccentFlags
        {
            None = 0x0,
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
            WCA_ACCENT_POLICY = 19,
            WCA_CORNER_STYLE = 27
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

        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_MAXIMIZEBOX = 0x10000;

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public uint length;
            public uint flags;
            public uint showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll", PreserveSig = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPlacement(
            IntPtr hWnd,
            in WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", PreserveSig = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(
            IntPtr hWnd,
            out WINDOWPLACEMENT lpwndpl);
    }
}
