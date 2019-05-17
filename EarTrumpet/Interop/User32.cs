using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EarTrumpet.Interop
{
    public class User32
    {
        public const int WM_USER = 0x0400;
        public const int WM_HOTKEY = 0x0312;

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

        [Flags]
        public enum WindowPosFlags : uint
        {
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0x0002,
            SWP_NOZORDER = 0x0004,
            SWP_NOACTIVATE = 0x0010,
        }

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            WindowPosFlags uFlags);

        [Flags]
        public enum MONITOR_DEFAULT : uint
        {
            MONITOR_DEFAULTTONULL = 0,
            MONITOR_DEFAULTTOPRIMARY = 1,
            MONITOR_DEFAULTTONEAREST = 2,
        }

        [DllImport("user32.dll", PreserveSig = true)]
        internal static extern IntPtr MonitorFromWindow(IntPtr hWnd, MONITOR_DEFAULT flags);

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

        public const int WS_EX_TOOLWINDOW = 0x00000080;

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTDEVICE
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

        [Flags]
        internal enum RAWMOUSE_FLAGS : ushort
        {
            MOUSE_MOVE_RELATIVE = 0,
            MOUSE_MOVE_ABSOLUTE = 1,
            // ...
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWMOUSE
        {
            [FieldOffset(0)]
            public RAWMOUSE_FLAGS usFlags;
            // union {
            [FieldOffset(4)]
            public uint ulButtons;
            // struct {
            [FieldOffset(4)]
            public ushort usButtonFlags;
            [FieldOffset(6)]
            public short usButtonData;
            // }
            // }
            [FieldOffset(8)]
            public uint ulRawButtons;
            [FieldOffset(12)]
            public int lLastX;
            [FieldOffset(16)]
            public int lLastY;
            [FieldOffset(20)]
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

        public enum HidUsagePage : ushort
        {
            /// <summary>Unknown usage page.</summary>
            UNDEFINED = 0x00,
            /// <summary>Generic desktop controls.</summary>
            GENERIC = 0x01,
            /// <summary>Simulation controls.</summary>
            SIMULATION = 0x02,
            /// <summary>Virtual reality controls.</summary>
            VR = 0x03,
            /// <summary>Sports controls.</summary>
            SPORT = 0x04,
            /// <summary>Games controls.</summary>
            GAME = 0x05,
            /// <summary>Keyboard controls.</summary>
            KEYBOARD = 0x07,
        }

        public enum HidUsage : ushort
        {
            /// <summary>Unknown usage.</summary>
            Undefined = 0x00,
            /// <summary>Pointer</summary>
            Pointer = 0x01,
            /// <summary>Mouse</summary>
            Mouse = 0x02,
            /// <summary>Joystick</summary>
            Joystick = 0x04,
            /// <summary>Game Pad</summary>
            Gamepad = 0x05,
            /// <summary>Keyboard</summary>
            Keyboard = 0x06,
            /// <summary>Keypad</summary>
            Keypad = 0x07,
            /// <summary>Multi-axis Controller</summary>
            SystemControl = 0x80,
            /// <summary>Tablet PC controls</summary>
            Tablet = 0x80,
            /// <summary>Consumer</summary>
            Consumer = 0x0C,
        }

        public enum GWL : int
        {
            // ...
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll", SetLastError = true, PreserveSig = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RegisterRawInputDevices(
            IntPtr rawInputDevices,
            uint numDevices,
            uint size);

        [DllImport("user32.dll", SetLastError = true, PreserveSig = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern uint GetRawInputData(
            IntPtr hRawInput,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader);


        internal const int RIDEV_NOLEGACY = 0x00000030;
        internal const int WM_INPUT = 0x00FF;
        internal const int RIDEV_INPUTSINK = 0x00000100;
        internal const int RID_INPUT = 0x10000003;
        internal const int RIDEV_REMOVE = 0x00000001;
        internal const int RIM_TYPEMOUSE = 0x0;
        internal const int RI_MOUSE_WHEEL = 0x0400;

        [DllImport("user32.dll", PreserveSig = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", PreserveSig = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyIcon(IntPtr iconHandle);

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", PreserveSig = true, CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms633576.aspx
        // The maximum length for lpszClassName is 256. If lpszClassName is greater than the maximum length, the RegisterClass function will fail.
        public static readonly int MAX_CLASSNAME_LENGTH = 256;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern IntPtr FindWindowEx(
            IntPtr hWndParent,
            IntPtr hWndChildAfter,
            [MarshalAs(UnmanagedType.LPWStr)]string lpClassName,
            IntPtr lpWindowName);

#if X86
        [DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern int GetWindowLong(
            IntPtr hWnd,
            GWL nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern int SetWindowLong(
            IntPtr hWnd,
            GWL nIndex,
            int dwNewLong);
#else
#error [Get/Set]WindowLong not supported on 64-bit platforms
#endif

        [DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern int RegisterWindowMessage(string msg);
    }
}
