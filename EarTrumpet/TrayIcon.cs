using EarTrumpet.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace EarTrumpet
{
    class TrayIcon
    {
        public event Action Invoked = delegate { };

        private readonly System.Windows.Forms.NotifyIcon _trayIcon;
        private const string _deviceSeparatorName = "DeviceSeparator";
        private const string _deviceItemPrefix = "Device_";
        private readonly EarTrumpetAudioDeviceService _audioDeviceService;

        public TrayIcon()
        {
            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            _audioDeviceService = new EarTrumpetAudioDeviceService();

            var aboutString = Properties.Resources.ContextMenuAboutTitle;
            var version = Assembly.GetEntryAssembly().GetName().Version;

            var deviceSep = _trayIcon.ContextMenu.MenuItems.Add("-");
            deviceSep.Name = _deviceSeparatorName;

            var aboutItem = _trayIcon.ContextMenu.MenuItems.Add(String.Format("{0} Ear Trumpet {1} ...", aboutString, version));
            aboutItem.Click += About_Click;            

            var exitItem = _trayIcon.ContextMenu.MenuItems.Add(EarTrumpet.Properties.Resources.ContextMenuExitTitle);
            exitItem.Click += Exit_Click;

            _trayIcon.MouseClick += TrayIcon_MouseClick;
            _trayIcon.ContextMenu.Popup += ContextMenu_Popup;
            _trayIcon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/EarTrumpet;component/Tray.ico")).Stream);
            _trayIcon.Text = string.Concat("Ear Trumpet - ", EarTrumpet.Properties.Resources.TrayIconTooltipText);
            _trayIcon.Visible = true;
        }

        private void ContextMenu_Popup(object sender, EventArgs e)
        {
            SetupDeviceMenuItems();
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

        void Exit_Click(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Current.Shutdown();
        }

        void Device_Click(object sender, EventArgs e)
        {
            var id = ((System.Windows.Forms.MenuItem)sender).Name.Substring(_deviceItemPrefix.Length);
            _audioDeviceService.SetDefaultAudioDevice(id);            
        }

        private void SetupDeviceMenuItems()
        {
            for (int i = _trayIcon.ContextMenu.MenuItems.Count - 1; i >= 0; i--)
            {
                var item = _trayIcon.ContextMenu.MenuItems[i];
                if (item.Name.StartsWith(_deviceItemPrefix))
                {
                    _trayIcon.ContextMenu.MenuItems.Remove(item);
                }
            }

            var audioDevices = _audioDeviceService.GetAudioDevices().ToList();
            for (int j = 0; j < audioDevices.Count; j++)
            {
                var device = audioDevices[j];
                var newItem = new System.Windows.Forms.MenuItem(device.DisplayName, Device_Click);
                newItem.Name = $"{_deviceItemPrefix}{device.Id}";
                newItem.Checked = device.IsDefault;
                _trayIcon.ContextMenu.MenuItems.Add(j, newItem);
            }
        }
    }
}
