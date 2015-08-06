using EarTrumpet.Services;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace EarTrumpet
{
    class TrayIcon
    {
        public event Action Invoked = delegate { };

        private readonly System.Windows.Forms.NotifyIcon _trayIcon;

        public TrayIcon()
        {
            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.ContextMenu = new System.Windows.Forms.ContextMenu();

            var aboutString = EarTrumpet.Properties.Resources.ContextMenuAboutTitle;
            var version = Assembly.GetEntryAssembly().GetName().Version;
            _trayIcon.ContextMenu.MenuItems.Add(String.Format("{0} Ear Trumpet {1} ...", aboutString, version));
            _trayIcon.ContextMenu.MenuItems[0].Click += About_Click;

            _trayIcon.ContextMenu.MenuItems.Add(EarTrumpet.Properties.Resources.ContextMenuShowDesktopAppsTitle);
            _trayIcon.ContextMenu.MenuItems[1].Checked = UserPreferencesService.ShowDesktopApps;
            _trayIcon.ContextMenu.MenuItems[1].Click += ShowDesktopApps_Click;
                
            _trayIcon.ContextMenu.MenuItems.Add("-");

            _trayIcon.ContextMenu.MenuItems.Add(EarTrumpet.Properties.Resources.ContextMenuExitTitle);
            _trayIcon.ContextMenu.MenuItems[3].Click += Exit_Click;

            _trayIcon.MouseClick += TrayIcon_MouseClick;
            _trayIcon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/EarTrumpet;component/Tray.ico")).Stream);
            _trayIcon.Text = "Ear Trumpet - Volume Control for Windows";
            _trayIcon.Visible = true;
        }

        void TrayIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Invoked.Invoke();
            }
        }

        void About_Click(object sender, EventArgs e)
        {
            Process.Start("http://github.com/File-New-Project/EarTrumpet");
        }

        void ShowDesktopApps_Click(object sender, EventArgs e)
        {
            var menuItem = (System.Windows.Forms.MenuItem)sender;
            menuItem.Checked = !menuItem.Checked;
            UserPreferencesService.ShowDesktopApps = menuItem.Checked;
        }

        void Exit_Click(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Current.Shutdown();
        }
    }
}
