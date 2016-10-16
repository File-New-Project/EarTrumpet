using EarTrumpet.Extensions;
using EarTrumpet.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;

namespace EarTrumpet
{
    public enum IconId
    {
        Muted = 120,
        SpeakerZeroBars = 121,
        SpeakerOneBar = 122,
        SpeakerTwoBars = 123,
        SpeakerThreeBars = 124,
        NoDevice = 125,
    }

    class TrayIcon
    {
        public event Action Invoked = delegate { };

        private readonly System.Windows.Forms.NotifyIcon _trayIcon;
        private const string _deviceSeparatorName = "DeviceSeparator";
        private const string _deviceItemPrefix = "Device_";
        private readonly string _trayIconPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\SndVolSSO.dll");
        private readonly AudioDeviceService _audioDeviceService;
        private AppServiceConnection _appServiceConnection;

        public TrayIcon()
        {
            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            _audioDeviceService = new AudioDeviceService();

            var aboutString = Properties.Resources.ContextMenuAboutTitle;
            var version = Assembly.GetEntryAssembly().GetName().Version;

            var deviceSep = _trayIcon.ContextMenu.MenuItems.Add("-");
            deviceSep.Name = _deviceSeparatorName;

            var feedbackItem = _trayIcon.ContextMenu.MenuItems.Add(EarTrumpet.Properties.Resources.ContextMenuSendFeedback);
            feedbackItem.Click += Feedback_Click;

            var aboutItem = _trayIcon.ContextMenu.MenuItems.Add(String.Format("{0} Ear Trumpet {1} ...", aboutString, version));
            aboutItem.Click += About_Click;

            var exitItem = _trayIcon.ContextMenu.MenuItems.Add(EarTrumpet.Properties.Resources.ContextMenuExitTitle);
            exitItem.Click += Exit_Click;

            _trayIcon.MouseClick += TrayIcon_MouseClick;
            _trayIcon.ContextMenu.Popup += ContextMenu_Popup;

            _trayIcon.Icon = IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerThreeBars);
            _trayIcon.Text = string.Concat("Ear Trumpet - ", EarTrumpet.Properties.Resources.TrayIconTooltipText);

            _trayIcon.Visible = true;
        }

        public void UpdateTrayIcon(bool noDevice = false, bool isMuted = false, int currentVolume = 100)
        {
            if (noDevice)
            {
                _trayIcon.Icon = IconService.GetIconFromFile(_trayIconPath, (int)IconId.NoDevice);
                return;
            }
            if (isMuted)
            {
                _trayIcon.Icon = IconService.GetIconFromFile(_trayIconPath, (int)IconId.Muted);
                return;
            }
            if (currentVolume == 0)
            {
                _trayIcon.Icon = IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerZeroBars);
                return;
            }
            if (currentVolume >= 1 && currentVolume <= 33)
            {
                _trayIcon.Icon = IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerOneBar);
                return;
            }
            if (currentVolume >= 34 && currentVolume <= 66)
            {
                _trayIcon.Icon = IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerTwoBars);
                return;
            }
            if (currentVolume >= 67 && currentVolume <= 100)
            {
                _trayIcon.Icon = IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerThreeBars);
                return;
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
        }

        void Feedback_Click(object sender, EventArgs e)
        {
            if (_appServiceConnection == null)
            {
                _appServiceConnection = new AppServiceConnection();
            }

            _appServiceConnection.AppServiceName = "SendFeedback";
            _appServiceConnection.PackageFamilyName = Package.Current.Id.FamilyName;
            _appServiceConnection.OpenAsync().Completed = AppServiceConnectionCompleted;
        }

        void AppServiceConnectionCompleted(IAsyncOperation<AppServiceConnectionStatus> operation, AsyncStatus asyncStatus)
        {
            var status = operation.GetResults();
            if (status == AppServiceConnectionStatus.Success)
            {
                var secondOperation = _appServiceConnection.SendMessageAsync(null);
                secondOperation.Completed = (_, __) =>
                {
                    _appServiceConnection.Dispose();
                    _appServiceConnection = null;
                };
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

            if (_appServiceConnection != null)
            {
                _appServiceConnection.Dispose();
            }
            
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
            if (audioDevices.Count == 0)
            {
                var newItem = new System.Windows.Forms.MenuItem(EarTrumpet.Properties.Resources.ContextMenuNoDevices);
                newItem.Name = $"{_deviceItemPrefix}";
                newItem.Enabled = false;
                _trayIcon.ContextMenu.MenuItems.Add(0, newItem);
            }
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
