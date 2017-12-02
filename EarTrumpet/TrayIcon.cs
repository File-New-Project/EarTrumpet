using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.ViewModels;
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
        private readonly TrayViewModel _trayViewModel;
        private AudioDeviceManager _deviceService;

        public TrayIcon(AudioDeviceManager deviceService, TrayViewModel trayViewModel)
        {
            _deviceService = deviceService;
            _deviceService.VirtualDefaultDevice.PropertyChanged += VirtualDefaultDevice_PropertyChanged;
            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            _trayViewModel = trayViewModel;
            _trayViewModel.PropertyChanged += TrayViewModel_PropertyChanged;

            var aboutString = Properties.Resources.ContextMenuAboutTitle;
            var version = Assembly.GetEntryAssembly().GetName().Version;

            _trayIcon.ContextMenu.MenuItems.Add("-");

            var feedbackItem = _trayIcon.ContextMenu.MenuItems.Add(EarTrumpet.Properties.Resources.ContextMenuSendFeedback);
            feedbackItem.Click += Feedback_Click;

            var aboutItem = _trayIcon.ContextMenu.MenuItems.Add(String.Format("{0} EarTrumpet {1} ...", aboutString, version));
            aboutItem.Click += About_Click;

            var exitItem = _trayIcon.ContextMenu.MenuItems.Add(EarTrumpet.Properties.Resources.ContextMenuExitTitle);
            exitItem.Click += Exit_Click;

            _trayIcon.MouseClick += TrayIcon_MouseClick;
            _trayIcon.ContextMenu.Popup += ContextMenu_Popup;

            _trayIcon.Icon = _trayViewModel.TrayIcon;

            UpdateToolTip();

            _trayIcon.Visible = true;
        }

        private void VirtualDefaultDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateToolTip();
        }

        void UpdateToolTip()
        {
            var otherText = "EarTrumpet: 100% ()";
            var dev = _deviceService.VirtualDefaultDevice.DisplayName;
            dev = dev.Substring(0, Math.Min(64 - otherText.Length, dev.Length));
            _trayIcon.Text = $"EarTrumpet: {_deviceService.VirtualDefaultDevice.Volume.ToVolumeInt()}% ({dev})";
        }

        private void TrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TrayIcon")
            {
                _trayIcon.Icon = _trayViewModel?.TrayIcon;
            }
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
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                _deviceService.VirtualDefaultDevice.IsMuted = !_deviceService.VirtualDefaultDevice.IsMuted;
            }
        }

        void Feedback_Click(object sender, EventArgs e)
        {
            _trayViewModel.StartAppServiceAndFeedbackHub();
        }

        void About_Click(object sender, EventArgs e)
        {
            Process.Start("http://github.com/File-New-Project/EarTrumpet");
        }

        void Exit_Click(object sender, EventArgs e)
        {
            try
            {
                foreach(var proc in Process.GetProcessesByName("EarTrumpet.UWP"))
                {
                    proc.Kill();
                }
            }
            catch
            {
                // We're shutting down, ignore all
            }
            _trayViewModel.CloseAppService();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Current.Shutdown();
        }

        void Device_Click(object sender, EventArgs e)
        {
            var device = (IAudioDevice)((System.Windows.Forms.MenuItem)sender).Tag;
            _deviceService.DefaultDevice = device;      
        }

        private void SetupDeviceMenuItems()
        {
            for (int i = _trayIcon.ContextMenu.MenuItems.Count - 1; i >= 0; i--)
            {
                var item = _trayIcon.ContextMenu.MenuItems[i];
                if (item.Tag != null)
                {
                    _trayIcon.ContextMenu.MenuItems.Remove(item);
                }
            }

            var audioDevices = _deviceService.Devices.OrderBy(x => x.DisplayName);
            if (audioDevices.Count() == 0)
            {
                var newItem = new System.Windows.Forms.MenuItem(EarTrumpet.Properties.Resources.ContextMenuNoDevices);
                newItem.Enabled = false;
                _trayIcon.ContextMenu.MenuItems.Add(0, newItem);
            }

            int iPos = 0;
            foreach(var device in audioDevices)
            {
                var newItem = new System.Windows.Forms.MenuItem(device.DisplayName, Device_Click);
                newItem.Tag = device;
                newItem.Checked = device == _deviceService.DefaultDevice;
                _trayIcon.ContextMenu.MenuItems.Add(iPos++, newItem);
            }
        }
    }
}
