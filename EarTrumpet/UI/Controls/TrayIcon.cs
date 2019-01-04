using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using EarTrumpet.Interop;
using Gma.System.MouseKeyHook;

namespace EarTrumpet.UI.Controls
{
    public class TrayIcon
    {
        private readonly NotifyIcon _trayIcon;
        private readonly ITrayViewModel _trayViewModel;
        private readonly IKeyboardMouseEvents _mouseGlobalHook;

        public TrayIcon(ITrayViewModel trayViewModel)
        {
            _mouseGlobalHook = Hook.GlobalEvents();

            _trayViewModel = trayViewModel;
            _trayViewModel.PropertyChanged += TrayViewModel_PropertyChanged;
            _trayViewModel.ContextMenuRequested += OnContextMenuRequested;

            _trayIcon = new NotifyIcon();
            _trayIcon.MouseClick += TrayIcon_MouseClick;
            _trayIcon.Icon = _trayViewModel.TrayIcon;
            _trayIcon.Text = _trayViewModel.ToolTip;

            _mouseGlobalHook.MouseWheel += MouseGlobalHook_MouseWheel;

            App.Current.Exit += App_Exit;
        }

        public bool IsVisible
        {
            get => _trayIcon.Visible;
            set
            {
                Trace.WriteLine("TrayIcon Show");
                _trayIcon.Visible = value;
                Trace.WriteLine("TrayIcon Shown");
            }
        }

        private void OnContextMenuRequested()
        {
            Trace.WriteLine("TrayIcon OnContextMenuRequested");
            var contextMenu = ThemedContextMenu.CreateThemedContextMenu(ThemeKind.DarkOnly);
            contextMenu.ItemsSource = _trayViewModel.MenuItems;
            contextMenu.Placement = PlacementMode.Mouse;
            contextMenu.IsOpen = true;
            Trace.WriteLine("TrayIcon OnContextMenuRequested (ContextMenu now open)");
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }

        private void TrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_trayViewModel.TrayIcon))
            {
                _trayIcon.Icon = _trayViewModel.TrayIcon;
            }
            else if (e.PropertyName == nameof(_trayViewModel.ToolTip))
            {
                _trayIcon.Text = _trayViewModel.ToolTip;
            }
        }

        private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            Trace.WriteLine($"TrayIcon TrayIcon_MouseClick {e.Button}");

            if (e.Button == MouseButtons.Left)
            {
                _trayViewModel.LeftClick.Execute();
            }
            else if (e.Button == MouseButtons.Right)
            {
                _trayViewModel.RightClick.Execute();
            }
            else if (e.Button == MouseButtons.Middle)
            {
                _trayViewModel.MiddleClick.Execute();
            }
        }

        private void MouseGlobalHook_MouseWheel(object sender, MouseEventArgs e)
        {
            var notifyIconLocation = NotifyIconInfo.GetNotifyIconLocation(_trayIcon);

            if (notifyIconLocation.left <= Cursor.Position.X &&
                notifyIconLocation.right >= Cursor.Position.Y &&
                notifyIconLocation.top <= Cursor.Position.Y &&
                notifyIconLocation.bottom >= Cursor.Position.Y)
            {
                if (e.Delta > 0)
                {
                    // Volume Up
                }
                else
                {
                    // Volume Down
                }
            }
        }
    }
}
