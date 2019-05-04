using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace EarTrumpet.Interop.Helpers
{
    class ShellNotifyIcon : IDisposable
    {
        public event MouseEventHandler MouseClick;

        public Icon Icon
        {
            get => _icon;
            set
            {
                if (value != _icon)
                {
                    _icon = value;
                    Update();
                }
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                if (value != _text)
                {
                    _text = value;
                    Update();
                }
            }
        }

        public bool Visible
        {
            get => _isVisible;
            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    Update();
                }
            }
        }

        private const int WM_CALLBACKMOUSEMSG = User32.WM_USER + 1024;
        private readonly int WM_TASKBARCREATED = User32.RegisterWindowMessage("TaskbarCreated");

        private readonly Func<Guid> _getIdentity;
        private readonly Action _resetIdentity;
        private Win32Window _window;
        private bool _isCreated;
        private bool _isVisible;
        private Icon _icon;
        private string _text;

        public ShellNotifyIcon(Func<Guid> getIdentity, Action resetIdentity)
        {
            _getIdentity = getIdentity;
            _resetIdentity = resetIdentity;
            _window = new Win32Window();
            _window.Initialize(WndProc);
        }

        public void SetFocus()
        {
            Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_SETFOCUS, MakeData());
        }

        public void Dispose()
        {
            if (_isVisible && _isCreated)
            {
                Visible = false;
            }

            _text = null;

            if (_window != null)
            {
                _window.Dispose();
                _window = null;
            }

            if (_icon != null)
            {
                _icon.Dispose();
                _icon = null;
            }
        }

        private void Update()
        {
            if (_isVisible)
            {
                if (!Shell32.Shell_NotifyIconW(_isCreated ? Shell32.NotifyIconMessage.NIM_MODIFY : Shell32.NotifyIconMessage.NIM_ADD, MakeData()))
                {
                    Trace.WriteLine("ShellNotifyIcon Update Failed 1");

                    _resetIdentity();
                    if (!Shell32.Shell_NotifyIconW(_isCreated ? Shell32.NotifyIconMessage.NIM_MODIFY : Shell32.NotifyIconMessage.NIM_ADD, MakeData()))
                    {
                        Trace.WriteLine("ShellNotifyIcon Update Failed 2");
                    }
                }
                _isCreated = true;
            }
            else if (_isCreated)
            {
                Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_DELETE, MakeData());
                _isCreated = false;
            }
        }

        private NOTIFYICONDATAW MakeData()
        {
            return new NOTIFYICONDATAW
            {
                hWnd = _window.Handle,
                uFlags = NotifyIconFlags.NIF_MESSAGE | NotifyIconFlags.NIF_ICON | NotifyIconFlags.NIF_TIP | NotifyIconFlags.NIF_GUID,
                uCallbackMessage = WM_CALLBACKMOUSEMSG,
                hIcon = Icon.Handle,
                szTip = Text,
                guidItem = _getIdentity(),
            };
        }

        private void WndProc(Message msg)
        {
            if (msg.Msg == WM_CALLBACKMOUSEMSG)
            {
                CallbackMsgWndProc(msg);
            }
            else if (msg.Msg == WM_TASKBARCREATED)
            {
                // System.Windows.Forms.NotifyIcon does this update in response to taskbar creation.
                Update();
            }
        }

        private void CallbackMsgWndProc(Message msg)
        {
            const int WM_LBUTTONUP = 0x0202;
            const int WM_RBUTTONUP = 0x0205;
            const int WM_MBUTTONUP = 0x0208;

            switch ((int)msg.LParam)
            {
                case WM_LBUTTONUP:
                    MouseClick(this, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    break;
                case WM_MBUTTONUP:
                    MouseClick(this, new MouseEventArgs(MouseButtons.Middle, 1, 0, 0, 0));
                    break;
                case WM_RBUTTONUP:
                    MouseClick(this, new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0));
                    break;
            }
        }
    }
}
