using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace EarTrumpet.UI.Controls
{
    class TrayIcon
    {
        private readonly NotifyIcon _trayIcon;
        private readonly ITrayViewModel _trayViewModel;

        public TrayIcon(ITrayViewModel trayViewModel)
        {
            _trayViewModel = trayViewModel;
            _trayViewModel.PropertyChanged += TrayViewModel_PropertyChanged;
            _trayViewModel.ContextMenuRequested += OnContextMenuRequested;

            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.MouseClick += TrayIcon_MouseClick;
            _trayIcon.Icon = _trayViewModel.TrayIcon;
            _trayIcon.Text = _trayViewModel.ToolTip;

            App.Current.Exit += App_Exit;
        }

        private void OnContextMenuRequested()
        {
            var cm = ThemedContextMenu.CreateThemedContextMenu(ThemeKind.DarkOnly);
            cm.ItemsSource = _trayViewModel.MenuItems;
            cm.Placement = PlacementMode.Mouse;
            cm.IsOpen = true;
            Trace.WriteLine("TrayIcon OnContextMenuRequested (ContextMenu now open)");
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }

        public void Show()
        {
            Trace.WriteLine("TrayIcon Show");
            _trayIcon.Visible = true;
            Trace.WriteLine("TrayIcon Shown");
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

        void TrayIcon_MouseClick(object sender, MouseEventArgs e)
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
    }
}
