using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using EarTrumpet.DataModel;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using Windows.Win32;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace EarTrumpet.UI.Helpers
{
    public sealed class ShellNotifyIcon : IDisposable
    {
        public class SecondaryInvokeArgs
        {
            public InputType InputType { get; set; }
            public Point Point { get; set; }
        }

        public event EventHandler<InputType> PrimaryInvoke;
        public event EventHandler<SecondaryInvokeArgs> SecondaryInvoke;
        public event EventHandler<InputType> TertiaryInvoke;
        public event EventHandler<int> Scrolled;

        public IShellNotifyIconSource IconSource { get; private set; }
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

        // Window messages should be int https://github.com/microsoft/win32metadata/pull/1769
        private const int WM_CALLBACKMOUSEMSG = (int)PInvoke.WM_USER + 1024;

        private readonly Win32Window _window;
        private readonly DispatcherTimer _invalidationTimer;
        private bool _isCreated;
        private bool _isVisible;
        private bool _isListeningForInput;
        private bool _isContextMenuOpen;
        private string _text;
        private Interop.RECT _iconLocation;
        private System.Drawing.Point _cursorPosition;
        private int _remainingTicks;
        private bool _hasAlreadyProcessedButtonUp;
        private bool HasAlreadyProcessedButtonUp
        {
            get
            {
                var val = _hasAlreadyProcessedButtonUp;
                _hasAlreadyProcessedButtonUp = false;
                return val;
            }
            set => _hasAlreadyProcessedButtonUp = value;
        }

        public ShellNotifyIcon(IShellNotifyIconSource icon)
        {
            IconSource = icon;
            IconSource.Changed += (_) => Update();
            _window = new Win32Window();
            _window.Initialize(WndProc);
            _invalidationTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, (_, __) => OnDelayedIconCheckForUpdate(), Dispatcher.CurrentDispatcher);

            Themes.Manager.Current.PropertyChanged += (_, __) => ScheduleDelayedIconInvalidation();
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (_, __) => ScheduleDelayedIconInvalidation();
        }

        public void SetFocus()
        {
            Trace.WriteLine("ShellNotifyIcon SetFocus");
            var data = MakeData();

            if (!PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_SETFOCUS, data))
            {
                Trace.WriteLine($"ShellNotifyIcon NIM_SETFOCUS Failed: {(uint)Marshal.GetLastWin32Error()}");
            }
        }

        public void SetTooltip(string text)
        {
            _text = text;
            Update();
        }

        private unsafe NOTIFYICONDATAW MakeData()
        {
            return new NOTIFYICONDATAW
            {
                cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATAW)),
                hWnd = new HWND(_window.Handle.ToPointer()),
                uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP | NOTIFY_ICON_DATA_FLAGS.NIF_SHOWTIP,
                uCallbackMessage = WM_CALLBACKMOUSEMSG,
                hIcon = new HICON(IconSource.Current.Handle.ToPointer()),
                szTip = _text
            };
        }

        private void Update()
        {
            var data = MakeData();
            if (_isVisible)
            {
                if (_isCreated)
                {
                    if (!PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, data))
                    {
                        // Modification will fail when the shell restarts, or if message processing times out
                        Trace.WriteLine($"ShellNotifyIcon Update NIM_MODIFY Failed: {(uint)Marshal.GetLastWin32Error()}");
                        _isCreated = false;
                        Update();
                    }
                }
                else
                {
                    if (!PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, data))
                    {
                        Trace.WriteLine($"ShellNotifyIcon Update NIM_ADD Failed {(uint)Marshal.GetLastWin32Error()}");
                    }

                    _isCreated = true;
                    data.Anonymous.uVersion = PInvoke.NOTIFYICON_VERSION_4;
                    if (!PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_SETVERSION, data))
                    {
                        Trace.WriteLine($"ShellNotifyIcon Update NIM_SETVERSION Failed: {(uint)Marshal.GetLastWin32Error()}");
                    }
                }
            }
            else if (_isCreated)
            {
                if (!PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, data))
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
                    (msg.Msg == PInvoke.WM_SETTINGCHANGE && (int)msg.WParam == (int)SYSTEM_PARAMETERS_INFO_ACTION.SPI_SETWORKAREA))
            {
                ScheduleDelayedIconInvalidation();
            }
            else if (msg.Msg == (int)PInvoke.WM_INPUT)
            {
                _cursorPosition = System.Windows.Forms.Cursor.Position;
                if (InputHelper.ProcessMouseInputMessage(msg.LParam, ref _cursorPosition, out var wheelDelta) &&
                                IsCursorWithinNotifyIconBounds() && wheelDelta != 0)
                {
                    Scrolled?.Invoke(this, wheelDelta);
                }
            }
        }

        private void CallbackMsgWndProc(System.Windows.Forms.Message msg)
        {
            switch ((short)msg.LParam)
            {
                case (short)PInvoke.NIN_SELECT:
                case (short)PInvoke.WM_LBUTTONUP:
                    // Observed double WM_CALLBACKMOUSEMSG/WM_LBUTTONUP pairs on Windows 11 22533
                    // Could be a result of XAML island use in Taskbar. Or a bug elsewhere.
                    // For now, swallow the duplicate to improve flyout UX.
                    if (!HasAlreadyProcessedButtonUp)
                    {
                        HasAlreadyProcessedButtonUp = true;
                        PrimaryInvoke?.Invoke(this, InputType.Mouse);
                    }
                    break;
                case PInvoke.NIN_KEYSELECT:
                    PrimaryInvoke?.Invoke(this, InputType.Keyboard);
                    break;
                case (short)PInvoke.WM_MBUTTONUP:
                    TertiaryInvoke?.Invoke(this, InputType.Mouse);
                    break;
                case (short)PInvoke.WM_CONTEXTMENU:
                    SecondaryInvoke?.Invoke(this, CreateSecondaryInvokeArgs(InputType.Keyboard, msg.WParam));
                    break;
                case (short)PInvoke.WM_RBUTTONUP:
                    SecondaryInvoke?.Invoke(this, CreateSecondaryInvokeArgs(InputType.Mouse, msg.WParam));
                    break;
                case (short)PInvoke.WM_MOUSEMOVE:
                    OnNotifyIconMouseMove();
                    IconSource.CheckForUpdate();
                    break;
            }
        }

        private static SecondaryInvokeArgs CreateSecondaryInvokeArgs(InputType type, IntPtr wParam) => new()
        {
            InputType = type,
            Point = new Point((short)wParam.ToInt32(), wParam.ToInt32() >> 16)
        };

        private unsafe void OnNotifyIconMouseMove()
        {
            var id = new NOTIFYICONIDENTIFIER
            {
                cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONIDENTIFIER)),
                hWnd = new HWND(_window.Handle.ToPointer())
            };

            if (PInvoke.Shell_NotifyIconGetRect(id, out var location) == 0)
            {
                _iconLocation = new() {
                    Bottom = location.bottom,
                    Left = location.left,
                    Right = location.right,
                    Top = location.top
                };

                _cursorPosition = System.Windows.Forms.Cursor.Position;
                IsCursorWithinNotifyIconBounds();
            }
            else
            {
                _iconLocation = default;
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
                IconSource.OnMouseOverChanged(IsMouseOver);
            }

            return isInBounds;
        }

        private void ScheduleDelayedIconInvalidation()
        {
            _remainingTicks = 10;
            _invalidationTimer.Start();

            IconSource.CheckForUpdate();
        }

        private void OnDelayedIconCheckForUpdate()
        {
            _remainingTicks--;
            if (_remainingTicks <= 0)
            {
                _invalidationTimer.Stop();
                // Force a final update to protect us from the shell doing implicit work
                Update();
            }

            IconSource.CheckForUpdate();
        }

        public void ShowContextMenu(IEnumerable itemsSource, Point point)
        {
            if (!_isContextMenuOpen)
            {
                _isContextMenuOpen = true;
                Trace.WriteLine("ShellNotifyIcon ShowContextMenu");
                var contextMenu = new ContextMenu
                {
                    FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                    StaysOpen = true,
                    ItemsSource = itemsSource
                };

                if (point.X > 0 && point.Y > 0)
                {
                    contextMenu.Placement = PlacementMode.Top;
                    contextMenu.PlacementRectangle = Rect.Empty;
                    contextMenu.PlacementTarget = null;
                    contextMenu.HorizontalOffset = point.X / (WindowsTaskbar.Dpi / (double)96);
                    contextMenu.VerticalOffset = point.Y / (WindowsTaskbar.Dpi / (double)96);
                }

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
                
                    unsafe
                    {
                        // Workaround: The framework expects there to already be a WPF window open and thus fails to take focus.
                        PInvoke.SetForegroundWindow(new HWND(((HwndSource)HwndSource.FromVisual(contextMenu)).Handle.ToPointer()));
                    }

                    contextMenu.Focus();
                    contextMenu.StaysOpen = false;

                    // Disable only the exit animation.
                    ((Popup)contextMenu.Parent).PopupAnimation = PopupAnimation.None;
                    };
                    contextMenu.Closed += (_, __) =>
                    {
                        Trace.WriteLine("ShellNotifyIcon ContextMenu.Closed");
                        _isContextMenuOpen = false;
                    };
                    contextMenu.IsOpen = true;
            }
        }

        public void Dispose()
        {
            _window.Dispose();
        }
    }
}