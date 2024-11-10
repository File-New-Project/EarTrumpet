using EarTrumpet.Interop.Helpers;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace EarTrumpet.Actions.Interop.Helpers;

internal class WindowWatcher : IDisposable
{
    public event Action<IntPtr> WindowCreated;
    public event Action<IntPtr> WindowDestroyed;

    private readonly Win32Window _window;
    private readonly uint _ShellNotifyMsg;

    public WindowWatcher()
    {
        _window = new Win32Window();
        _window.Initialize(WndProc);
        _ShellNotifyMsg = User32.RegisterWindowMessageW(User32.SHELLHOOK);
        if (!User32.RegisterShellHookWindow(_window.Handle))
        {
            Trace.WriteLine("Failed to register shell hook window");
        }
    }

    private void WndProc(Message m)
    {
        if (m.Msg == _ShellNotifyMsg)
        {
            if (m.WParam.ToInt32() == User32.HSHELL_WINDOWCREATED)
            {
                WindowCreated?.Invoke(m.LParam);
            }
            else if (m.WParam.ToInt32() == User32.HSHELL_WINDOWDESTROYED)
            {
                WindowDestroyed?.Invoke(m.LParam);
            }
        }
    }

    public void Dispose()
    {
        _window.Dispose();
    }
}
