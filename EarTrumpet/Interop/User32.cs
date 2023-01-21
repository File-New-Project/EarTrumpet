using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace EarTrumpet.Interop
{
    public class User32
    {
        public const int WM_USER = 0x0400;
        public const int WM_HOTKEY = 0x0312;
        public const int WM_CONTEXTMENU = 0x007B;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_SETTINGCHANGE = 0x001A;
        public const int SPI_SETWORKAREA = 0x002F;

        public static uint MAKEWPARAM(ushort low, ushort high) => ((uint)high << 16) | low;

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

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTDEVICE
        {
            public User32.HidUsagePage usUsagePage;
            public User32.HidUsage usUsage;
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
            MOUSE_VIRTUAL_DESKTOP = 2,
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

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUT
        {
            public RAWINPUTHEADER header;
            public RAWMOUSE mouse;
            // ...
        }

        public enum HidUsagePage : ushort
        {
            UNDEFINED = 0x00,
            GENERIC = 0x01,
            SIMULATION = 0x02,
            VR = 0x03,
            SPORT = 0x04,
            GAME = 0x05,
            KEYBOARD = 0x07,
        }

        [SuppressMessage("Design", "CA1069:Enums values should not be duplicated")]
        [SuppressMessage("Naming", "CA1720:Identifier contains type name")]
        public enum HidUsage : ushort
        {
            Undefined = 0x00,
            Pointer = 0x01,
            Mouse = 0x02,
            Joystick = 0x04,
            Gamepad = 0x05,
            Keyboard = 0x06,
            Keypad = 0x07,
            SystemControl = 0x80,
            Tablet = 0x80,
            Consumer = 0x0C,
        }

        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
        [SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name")]
        public enum GWL : int
        {
            // ...
            GWL_STYLE = -16,
            GWL_EXSTYLE = -20,
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
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", PreserveSig = true, CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
        public static readonly int MAX_CLASSNAME_LENGTH = 256;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        [SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
        public static extern IntPtr FindWindowEx(
            IntPtr hWndParent,
            IntPtr hWndChildAfter,
            [MarshalAs(UnmanagedType.LPWStr)]string lpClassName,
            IntPtr lpWindowName);

#if X86
        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Unicode, PreserveSig = true)]
#else
        [DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
#endif
        public static extern int GetWindowLongPtr(
            IntPtr hWnd,
            GWL nIndex);


#if X86
        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Unicode, PreserveSig = true)]
#else
        [DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
#endif
        public static extern int SetWindowLongPtr(
            IntPtr hWnd,
            GWL nIndex,
            int dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern int RegisterWindowMessage(string msg);

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern uint GetDpiForWindow(IntPtr hWnd);

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern uint GetDpiForSystem();

        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
        public enum SystemMetrics : int
        {
            // ...
            SM_CXICON = 11,
            SM_CYICON = 12,
            SM_CXSMICON = 49,
            SM_CYSMICON = 50,
            SM_CXVIRTUALSCREEN = 78,
            SM_CYVIRTUALSCREEN = 79,
            // ...
        }

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern int GetSystemMetricsForDpi(SystemMetrics nIndex, uint dpi);

        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
        [SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
        public enum LoadImageFlags : uint
        {
            // ...
            LR_DEFAULTCOLOR = 0x00000000,
            LR_SHARED = 0x00008000,
            // ...
        }

        public enum IconCursorVersion : int
        {
            Default = 0x00030000
        }

        [DllImport("user32.dll", PreserveSig = true, SetLastError = true)]
        [SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
        public static extern IntPtr CreateIconFromResourceEx(
            IntPtr presbits,
            int dwResSize,
            [MarshalAs(UnmanagedType.Bool)]bool fIcon,
            IconCursorVersion dwVer,
            int cxDesired,
            int cyDesired,
            LoadImageFlags Flags);

        [DllImport("user32.dll", PreserveSig = true)]
        [SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
        public static extern int LookupIconIdFromDirectoryEx(
            IntPtr presbits,
            [MarshalAs(UnmanagedType.Bool)]bool fIcon,
            int cxDesired,
            int cyDesired,
            LoadImageFlags Flags);

        [Flags]
        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
        public enum GR_FLAGS : uint
        {
            GR_GDIOBJECTS = 0,
            GR_USEROBJECTS = 1,
            GR_GDIOBJECTS_PEAK = 2,
            GR_USEROBJECTS_PEAK = 4
        }

        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
        public static readonly IntPtr GR_GLOBAL = new(-2);

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern uint GetGuiResources(
                IntPtr hProcess,
                GR_FLAGS uiFlags);
    }
}
