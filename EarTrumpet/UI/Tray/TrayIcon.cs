using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;

namespace EarTrumpet.UI.Tray
{
    public class TrayIcon
    {
        private readonly ShellNotifyIcon _trayIcon;
        private readonly ITrayViewModel _trayViewModel;

        public TrayIcon(ITrayViewModel trayViewModel)
        {
            _trayViewModel = trayViewModel;
            _trayViewModel.PropertyChanged += TrayViewModel_PropertyChanged;

            _trayIcon = new ShellNotifyIcon(trayViewModel.GetIdentity, trayViewModel.ResetIdentity);
            _trayIcon.MouseClick += TrayIcon_MouseClick;
            _trayIcon.Icon = _trayViewModel.TrayIcon;
            _trayIcon.Text = _trayViewModel.ToolTip;

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
            var contextMenu = TaskbarContextMenuHelper.Create();
            Themes.Options.SetSource(contextMenu, Themes.Options.SourceKind.System);
            contextMenu.ItemsSource = _trayViewModel.MenuItems;
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
                var oldIcon = _trayIcon.Icon;
                _trayIcon.Icon = _trayViewModel.TrayIcon;
                oldIcon?.Dispose();
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
                _trayViewModel.LeftClick.Execute(null);
            }
            else if (e.Button == MouseButtons.Right)
            {
                OnContextMenuRequested();
            }
            else if (e.Button == MouseButtons.Middle)
            {
                _trayViewModel.MiddleClick.Execute(null);
            }
        }
    }
}
