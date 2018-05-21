using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using EarTrumpet.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows;

namespace EarTrumpet.ViewModels
{
    public class TrayViewModel : BindableBase
    {
        public enum IconId
        {
            Muted = 120,
            SpeakerZeroBars = 121,
            SpeakerOneBar = 122,
            SpeakerTwoBars = 123,
            SpeakerThreeBars = 124,
            NoDevice = 125,
            OriginalIcon
        }

        private readonly string _trayIconPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\SndVolSSO.dll");
        private Icon _trayIcon;
        private readonly IAudioDeviceManager _deviceService;
        private readonly FlyoutViewModel _flyoutViewModel;
        private readonly Dictionary<IconId, Icon> _icons = new Dictionary<IconId, Icon>();
        private IconId _currentIcon = IconId.OriginalIcon;

        public Icon TrayIcon
        {
            get
            {
                return _trayIcon;
            }
            set
            {
                if (_trayIcon != value)
                {
                    _trayIcon = value;
                    RaisePropertyChanged(nameof(TrayIcon));
                }
            }
        }

        public RelayCommand OpenSettingsCommand { get; }
        public RelayCommand OpenPlaybackDevicesCommand { get; }
        public RelayCommand OpenRecordingDevicesCommand { get; }
        public RelayCommand OpenSoundsControlPanelCommand { get; }
        public RelayCommand OpenLegacyVolumeMixerCommand { get; }
        public RelayCommand OpenEarTrumpetVolumeMixerCommand { get; }
        public RelayCommand<IAudioDevice> ChangeDeviceCommand { get; }
        public RelayCommand StartAppServiceAndFeedbackHubCommand { get; }
        public RelayCommand OpenFlyoutCommand { get; }

        public TrayViewModel(IAudioDeviceManager deviceService, FlyoutViewModel flyoutViewModel)
        {
            _flyoutViewModel = flyoutViewModel;

            var originalIcon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/EarTrumpet;component/Assets/Tray.ico")).Stream);
            _icons.Add(IconId.OriginalIcon, originalIcon);
            try
            {
                _icons.Add(IconId.NoDevice, IconService.GetIconFromFile(_trayIconPath, (int)IconId.NoDevice));
                _icons.Add(IconId.Muted, IconService.GetIconFromFile(_trayIconPath, (int)IconId.Muted));
                _icons.Add(IconId.SpeakerZeroBars, IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerZeroBars));
                _icons.Add(IconId.SpeakerOneBar, IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerOneBar));
                _icons.Add(IconId.SpeakerTwoBars, IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerTwoBars));
                _icons.Add(IconId.SpeakerThreeBars, IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerThreeBars));
            }
            catch
            {
                _icons.Clear();
                _icons.Add(IconId.NoDevice, originalIcon);
                _icons.Add(IconId.Muted, originalIcon);
                _icons.Add(IconId.SpeakerZeroBars, originalIcon);
                _icons.Add(IconId.SpeakerOneBar, originalIcon);
                _icons.Add(IconId.SpeakerTwoBars, originalIcon);
                _icons.Add(IconId.SpeakerThreeBars, originalIcon);

                _icons.Add(IconId.OriginalIcon, originalIcon);
            }

            _deviceService = deviceService;
            _deviceService.VirtualDefaultDevice.PropertyChanged += VirtualDefaultDevice_PropertyChanged;

            UpdateTrayIcon();

            OpenSettingsCommand = new RelayCommand(OpenSettings);
            OpenPlaybackDevicesCommand = new RelayCommand(OpenPlaybackDevices);
            OpenRecordingDevicesCommand = new RelayCommand(OpenRecordingDevices);
            OpenSoundsControlPanelCommand = new RelayCommand(OpenSoundsControlPanel);
            OpenLegacyVolumeMixerCommand = new RelayCommand(OpenLegacyVolumeMixer);
            OpenEarTrumpetVolumeMixerCommand = new RelayCommand(OpenEarTrumpetVolumeMixer);
            ChangeDeviceCommand = new RelayCommand<IAudioDevice>(ChangeDevice);
            StartAppServiceAndFeedbackHubCommand = new RelayCommand(FeedbackService.StartAppServiceAndFeedbackHub);
            OpenFlyoutCommand = new RelayCommand(OpenFlyout);
        }

        private void VirtualDefaultDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateTrayIcon();
        }

        void UpdateTrayIcon()
        {
            bool noDevice = !_deviceService.VirtualDefaultDevice.IsDevicePresent;
            bool isMuted = _deviceService.VirtualDefaultDevice.IsMuted;
            int currentVolume = _deviceService.VirtualDefaultDevice.Volume.ToVolumeInt();

            if (noDevice)
            {
                _currentIcon = IconId.NoDevice;
                TrayIcon = _icons[IconId.NoDevice];
                return;
            }
            else if (isMuted)
            {
                _currentIcon = IconId.Muted;
                TrayIcon = _icons[IconId.Muted];
                return;
            }
            else if (currentVolume == 0)
            {
                _currentIcon = IconId.SpeakerZeroBars;
                TrayIcon = _icons[IconId.SpeakerZeroBars];
                return;
            }
            else if (currentVolume >= 1 && currentVolume < 33)
            {
                _currentIcon = IconId.SpeakerOneBar;
                TrayIcon = _icons[IconId.SpeakerOneBar];
                return;
            }
            else if (currentVolume >= 33 && currentVolume < 66)
            {
                _currentIcon = IconId.SpeakerTwoBars;
                TrayIcon = _icons[IconId.SpeakerTwoBars];
                return;
            }
            else if (currentVolume >= 66 && currentVolume <= 100)
            {
                _currentIcon = IconId.SpeakerThreeBars;
                TrayIcon = _icons[IconId.SpeakerThreeBars];
                return;
            }

            if (_currentIcon == IconId.OriginalIcon)
            {
                TrayIcon = _icons[IconId.OriginalIcon];
                _currentIcon = IconId.OriginalIcon;
            }
        }

        private void OpenPlaybackDevices()
        {
            Process.Start("rundll32.exe", "shell32.dll,Control_RunDLL mmsys.cpl,,playback");
        }

        private void OpenRecordingDevices()
        {
            Process.Start("rundll32.exe", "shell32.dll,Control_RunDLL mmsys.cpl,,recording");
        }

        private void OpenSoundsControlPanel()
        {
            Process.Start("rundll32.exe", "shell32.dll,Control_RunDLL mmsys.cpl,,sounds");
        }

        private void OpenLegacyVolumeMixer()
        {
            Process.Start("sndvol.exe");
        }

        private void OpenSettings()
        {
            if (SettingsWindow.Instance == null)
            {
                var window = new SettingsWindow(_deviceService);
                window.Show();
                WindowAnimationLibrary.BeginWindowEntranceAnimation(window, () => { });

            }
            else
            {
                SettingsWindow.Instance.RaiseWindow();
            }
        }

        private void OpenEarTrumpetVolumeMixer()
        {
            if (FullWindow.Instance == null)
            {
                var window = new FullWindow(MainViewModel.Instance);

                window.Show();
                WindowAnimationLibrary.BeginWindowEntranceAnimation(window, () => { });

            }
            else
            {
                FullWindow.Instance.RaiseWindow();
            }
        }

        private void ChangeDevice(IAudioDevice device)
        {
            _deviceService.DefaultPlaybackDevice = device;
        }

        private void OpenFlyout()
        {
            _flyoutViewModel.OpenFlyoutFromExternal();
        }
    }
}
