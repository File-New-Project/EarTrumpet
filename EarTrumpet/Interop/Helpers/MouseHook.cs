using System.Runtime.InteropServices;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace EarTrumpet.Interop.Helpers;

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

    private HOOKPROC _hProc;
    private HHOOK _hHook;
    private bool _hookIsSet = false;

    public void SetHook()
    {
        if (!_hookIsSet)
        {
            _hProc = new HOOKPROC(MouseHookProc);
            _hHook = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_MOUSE_LL, _hProc, (HINSTANCE)null, 0);
            _hookIsSet = true;
        }
    }

    public void UnHook()
    {
        if (_hookIsSet)
        {
            PInvoke.UnhookWindowsHookEx(_hHook);
            _hookIsSet = false;
        }
    }

    private LRESULT MouseHookProc(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode < 0 || MouseWheelEvent == null || wParam != PInvoke.WM_MOUSEWHEEL)
        {
            return PInvoke.CallNextHookEx(_hHook, nCode, wParam, lParam);
        }
        var MyMouseHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));
        var result = MouseWheelEvent(this, new MouseEventArgs(MouseButtons.None, 0, MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y, MyMouseHookStruct.mouseData >> 16));
        if (result == 0)
        {
            return PInvoke.CallNextHookEx(_hHook, nCode, wParam, lParam);
        }
        return new LRESULT(result);
    }
}
