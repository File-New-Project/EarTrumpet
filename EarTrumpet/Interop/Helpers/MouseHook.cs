using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.Helpers
{
    public class MouseHook
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MouseLLHookStruct
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        public delegate int MouseWheelHandler(object sender, MouseEventArgs e);
        public event MouseWheelHandler MouseWheelEvent;

        public delegate int MiddleClickHandler(object sender, MouseEventArgs e);
        public event MiddleClickHandler MiddleClickEvent;

        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WH_MOUSE_LL = 14;
        private User32.HookProc _hProc;
        private int _hHook;
        private bool _hookIsSet = false;

        public void SetHook()
        {
            if (!_hookIsSet)
            {
                _hProc = new User32.HookProc(MouseHookProc);
                _hHook = User32.SetWindowsHookEx(WH_MOUSE_LL, _hProc, IntPtr.Zero, 0);
                _hookIsSet = true;
            }
        }

        public void UnHook()
        {
            if (_hookIsSet)
            {
                User32.UnhookWindowsHookEx(_hHook);
                _hookIsSet = false;
            }
        }

        private int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                return User32.CallNextHookEx(_hHook, nCode, wParam, lParam);
            }

            int msgType = (Int32)wParam;

            if (msgType == WM_MOUSEWHEEL && MouseWheelEvent != null)
            {
                MouseLLHookStruct MyMouseHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));
                int result = MouseWheelEvent(this, new MouseEventArgs(MouseButtons.None, 0, MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y, MyMouseHookStruct.mouseData >> 16));
                if (result != 0)
                {
                    return result;
                }
            }

            if (msgType == WM_MBUTTONDOWN && MiddleClickEvent != null)
            {
                MouseLLHookStruct MyMouseHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));
                int result = MiddleClickEvent(this, new MouseEventArgs(MouseButtons.Middle, 1, MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y, 0));
                if (result != 0)
                {
                    return result;
                }
            }

            return User32.CallNextHookEx(_hHook, nCode, wParam, lParam);
        }
    }
}
