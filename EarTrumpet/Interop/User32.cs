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
        public static extern bool RegisterHotKey(
            IntPtr hWnd,
            int id,
            uint fsModifiers,
            uint vk);

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern bool UnregisterHotKey(
            IntPtr hWnd,
            int id);

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            uint uFlags);

        [DllImport("user32.dll", PreserveSig = true)]
        internal static extern int SetWindowCompositionAttribute(
            IntPtr hwnd,
            ref WindowCompositionAttribData data);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern IntPtr FindWindow(
            [MarshalAs(UnmanagedType.LPWStr)]string lpClassName,
            string lpWindowName);

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern bool GetWindowRect(
            IntPtr hwnd,
            out RECT lpRect);

        [DllImport("user32.dll", PreserveSig = true)]
        internal static extern int IsImmersiveProcess(
            IntPtr hProcess);

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

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTDEV
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        };

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        };

        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWMOUSE
        {
            [FieldOffset(0)]
            public ushort usFlags;
            [FieldOffset(2)]
            public uint ulButtons;
            [FieldOffset(4)]
            public ushort usButtonFlags;
            [FieldOffset(6)]
            public short usButtonData;
            [FieldOffset(6)]
            public uint ulRawButtons;
            [FieldOffset(10)]
            public int lLastX;
            [FieldOffset(14)]
            public int lLastY;
            [FieldOffset(18)]
            public uint ulExtraInformation;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWINPUT
        {
            [FieldOffset(0)]
            public RAWINPUTHEADER header;

            [FieldOffset(16)]
            public RAWMOUSE mouse;

            // ...
        }


        [DllImport("User32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RegisterRawInputDevices(IntPtr rawInputDevices, uint numDevices, uint size);

        [DllImport("User32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern int GetRawInputData(IntPtr hRawInput,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader
            );

        internal const int RIDEV_NOLEGACY = 0x00000030;
        internal const int WM_INPUT = 0x00FF;
        internal const int RIDEV_INPUTSINK = 0x00000100;
        internal const int RID_INPUT = 0x10000003;
        internal const int RIDEV_REMOVE = 0x00000001;
        internal const int RIM_TYPEMOUSE = 0x0;
        internal const int RI_MOUSE_WHEEL = 0x0400;
    }
}
