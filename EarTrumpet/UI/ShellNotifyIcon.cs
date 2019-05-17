using EarTrumpet.DataModel;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace EarTrumpet.UI
{
    public class ShellNotifyIcon
    {
        public event EventHandler<InputType> PrimaryInvoke;
        public event EventHandler<InputType> SecondaryInvoke;
        public event EventHandler<InputType> TertiaryInvoke;
        public event EventHandler<int> Scrolled;
        
        public bool IsMouseOver { get; private set; }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    Update();
                    Trace.WriteLine($"ShellNotifyIcon IsVisible {_isVisible}");
                }
            }
        }

        private const int WM_CALLBACKMOUSEMSG = User32.WM_USER + 1024;

        private readonly Func<Guid> _getIdentity;
        private readonly Action _resetIdentity;
        private Win32Window _window;
        private bool _isCreated;
        private bool _isVisible;
        private bool _isListeningForInput;
        public IconSource IconSource { get; private set; }
        private string _text;
        private RECT _iconLocation;
        private POINT _cursorPosition;
        private DispatcherTimer _invalidationTimer;

        public ShellNotifyIcon(IconSource icon, Func<Guid> getIdentity, Action resetIdentity)
        {
            IconSource = icon;
            IconSource.Changed += (_) => Update();
            _getIdentity = getIdentity;
            _resetIdentity = resetIdentity;
            _window = new Win32Window();
            _window.Initialize(WndProc);
            _invalidationTimer = new DispatcherTimer(TimeSpan.FromSeconds(3), DispatcherPriority.Normal, (_, __) => OnDelayedIconCheckForUpdate(), Dispatcher.CurrentDispatcher);

            Themes.Manager.Current.PropertyChanged += (_, __) => ScheduleDelayedIconInvalidation();
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (_, __) => ScheduleDelayedIconInvalidation();
        }

        public void SetFocus()
        {
            Trace.WriteLine("ShellNotifyIcon SetFocus");
            var data = MakeData();
            if (!Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_SETFOCUS, ref data))
            {
                Trace.WriteLine($"ShellNotifyIcon NIM_SETFOCUS Failed: {(uint)Marshal.GetLastWin32Error()}");
            }
        }

        public void SetTooltip(string text)
        {
            _text = text;
            Update();
        }

        private NOTIFYICONDATAW MakeData()
        {
            return new NOTIFYICONDATAW
            {
                cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATAW)),
                hWnd = _window.Handle,
                uFlags = NotifyIconFlags.NIF_MESSAGE | NotifyIconFlags.NIF_ICON | NotifyIconFlags.NIF_TIP | NotifyIconFlags.NIF_GUID,
                uCallbackMessage = WM_CALLBACKMOUSEMSG,
                hIcon = IconSource.Current.Handle,
                szTip = _text,
                guidItem = _getIdentity(),
            };
        }

        private void Update()
        {
            var data = MakeData();
            if (_isVisible)
            {
                if (_isCreated)
                {
                    if (!Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_MODIFY, ref data))
                    {
                        Trace.WriteLine($"ShellNotifyIcon Update NIM_MODIFY Failed: {(uint)Marshal.GetLastWin32Error()}");
                    }
                }
                else
                {
                    if (!Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_ADD, ref data))
                    {
                        Trace.WriteLine($"ShellNotifyIcon Update NIM_ADD Failed (first attempt) {(uint)Marshal.GetLastWin32Error()}");

                        _resetIdentity();
                        data = MakeData();
                        if (!Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_ADD, ref data))
                        {
                            Trace.WriteLine($"### ShellNotifyIcon Update NIM_ADD Failed: {(uint)Marshal.GetLastWin32Error()} ###");
                        }
                    }
                    _isCreated = true;

                    data.uTimeoutOrVersion = Shell32.NOTIFYICON_VERSION_4;
                    if (!Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_SETVERSION, ref data))
                    {
                        Trace.WriteLine($"ShellNotifyIcon Update NIM_SETVERSION Failed: {(uint)Marshal.GetLastWin32Error()}");
                    }
                }
            }
            else if (_isCreated)
            {
                if (!Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_DELETE, ref data))
                {
                    Trace.WriteLine($"ShellNotifyIcon Update NIM_DELETE Failed: {(uint)Marshal.GetLastWin32Error()}");
                }
                _isCreated = false;
            }
        }

        private void WndProc(System.Windows.Forms.Message msg)
        {
            if (msg.Msg == WM_CALLBACKMOUSEMSG)
            {
                CallbackMsgWndProc(msg);
            }
            else if (msg.Msg == Shell32.WM_TASKBARCREATED ||
                    (msg.Msg == User32.WM_SETTINGCHANGE && (int)msg.WParam == User32.SPI_SETWORKAREA))
            {
                ScheduleDelayedIconInvalidation();
            }
            else if (msg.Msg == User32.WM_INPUT &&
                    InputHelper.ProcessMouseInputMessage(msg.LParam, ref _cursorPosition, out int wheelDelta) &&
                    IsCursorWithinNotifyIconBounds() && wheelDelta != 0)
            {
                Scrolled?.Invoke(this, wheelDelta);
            }
        }

        private void CallbackMsgWndProc(System.Windows.Forms.Message msg)
        {
            switch ((int)msg.LParam)
            {
                case (int)Shell32.NotifyIconNotification.NIN_SELECT:
                case User32.WM_LBUTTONUP:
                    PrimaryInvoke?.Invoke(this, InputType.Mouse);
                    break;
                case (int)Shell32.NotifyIconNotification.NIN_KEYSELECT:
                    PrimaryInvoke?.Invoke(this, InputType.Keyboard);
                    break;
                case User32.WM_MBUTTONUP:
                    TertiaryInvoke?.Invoke(this, InputType.Mouse);
                    break;
                case User32.WM_CONTEXTMENU:
                    SecondaryInvoke?.Invoke(this, InputType.Keyboard);
                    break;
                case User32.WM_RBUTTONUP:
                    SecondaryInvoke?.Invoke(this, InputType.Mouse);
                    break;
                case User32.WM_MOUSEMOVE:
                    OnNotifyIconMouseMove();
                    IconSource.CheckForUpdate();
                    break;
            }
        }

        private void OnNotifyIconMouseMove()
        {
            var id = new NOTIFYICONIDENTIFIER
            {
                cbSize = Marshal.SizeOf(typeof(NOTIFYICONIDENTIFIER)),
                guidItem = _getIdentity(),
            };

            if (Shell32.Shell_NotifyIconGetRect(ref id, out RECT location) == 0)
            {
                _iconLocation = location;

                if (User32.GetCursorPos(out POINT pt))
                {
                    _cursorPosition = pt;
                    IsCursorWithinNotifyIconBounds();
                }
            }
            else
            {
                _iconLocation = default(RECT);
            }
        }

        private bool IsCursorWithinNotifyIconBounds()
        {
            bool isInBounds = _iconLocation.Contains(_cursorPosition);
            if (isInBounds)
            {
                if (!_isListeningForInput)
                {
                    _isListeningForInput = true;
                    InputHelper.RegisterForMouseInput(_window.Handle);
                }
            }
            else
            {
                if (_isListeningForInput)
                {
                    _isListeningForInput = false;
                    InputHelper.UnregisterForMouseInput();
                }
            }

            bool isChanged = (IsMouseOver != isInBounds);
            IsMouseOver = isInBounds;
            if (isChanged)
            {
                IconSource.CheckForUpdate();
            }

            return isInBounds;
        }

        private void ScheduleDelayedIconInvalidation()
        {
            IconSource.CheckForUpdate();

            _invalidationTimer.Stop();
            _invalidationTimer.Start();
        }

        private void OnDelayedIconCheckForUpdate()
        {
            _invalidationTimer.Stop();
            IconSource.CheckForUpdate();
            Update();
        }

        public void ShowContextMenu(IEnumerable itemsSource)
        {
            Trace.WriteLine("ShellNotifyIcon ShowContextMenu");
            var contextMenu = new ContextMenu
            {
                FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                StaysOpen = true,
                ItemsSource = itemsSource,
            };
            Themes.Options.SetSource(contextMenu, Themes.Options.SourceKind.System);
            contextMenu.PreviewKeyDown += (_, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    SetFocus();
                }
            };
            contextMenu.Opened += (_, __) =>
            {
                Trace.WriteLine("ShellNotifyIcon ContextMenu.Opened");
                // Workaround: The framework expects there to already be a WPF window open and thus fails to take focus.
                User32.SetForegroundWindow(((HwndSource)HwndSource.FromVisual(contextMenu)).Handle);
                contextMenu.Focus();
                contextMenu.StaysOpen = false;
                // Disable only the exit animation.
                ((Popup)contextMenu.Parent).PopupAnimation = PopupAnimation.None;
            };
            contextMenu.Closed += (_, __) => Trace.WriteLine("ShellNotifyIcon ContextMenu.Closed"); ;
            contextMenu.IsOpen = true;
        }
    }
}