using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using EarTrumpet.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public Icon TrayIcon { get; private set; }
        public RelayCommand OpenSettingsCommand { get; }
        public RelayCommand OpenPlaybackDevicesCommand { get; }
        public RelayCommand OpenRecordingDevicesCommand { get; }
        public RelayCommand OpenSoundsControlPanelCommand { get; }
        public RelayCommand OpenLegacyVolumeMixerCommand { get; }
        public RelayCommand OpenEarTrumpetVolumeMixerCommand { get; }
        public RelayCommand<DeviceViewModel> ChangeDeviceCommand { get; }
        public RelayCommand StartAppServiceAndFeedbackHubCommand { get; }
        public RelayCommand OpenFlyoutCommand { get; }
        public RelayCommand ExitCommand { get; }

        private readonly string _trayIconPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\SndVolSSO.dll");
        private readonly IAudioDeviceManager _deviceManager;
        private readonly MainViewModel _mainViewModel;
        private readonly Dictionary<IconId, Icon> _icons = new Dictionary<IconId, Icon>();
        private IconId _currentIcon = IconId.OriginalIcon;

        public ObservableCollection<DeviceViewModel> AllDevices => _mainViewModel.AllDevices;

        internal TrayViewModel(MainViewModel mainViewModel, IAudioDeviceManager deviceManager)
        {
            _mainViewModel = mainViewModel;
            _deviceManager = deviceManager;
            _deviceManager.VirtualDefaultDevice.PropertyChanged += (_, __) => UpdateTrayIcon();

            var originalIcon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/EarTrumpet;component/Assets/Tray.ico")).Stream);
            try
            {
                _icons.Add(IconId.OriginalIcon, originalIcon);
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
                _icons.Add(IconId.OriginalIcon, originalIcon);
                _icons.Add(IconId.NoDevice, originalIcon);
                _icons.Add(IconId.Muted, originalIcon);
                _icons.Add(IconId.SpeakerZeroBars, originalIcon);
                _icons.Add(IconId.SpeakerOneBar, originalIcon);
                _icons.Add(IconId.SpeakerTwoBars, originalIcon);
                _icons.Add(IconId.SpeakerThreeBars, originalIcon);
            }

            UpdateTrayIcon();

            OpenSettingsCommand = new RelayCommand(SettingsWindow.ActivateSingleInstance);
            OpenPlaybackDevicesCommand = new RelayCommand(() => OpenControlPanel("playback"));
            OpenRecordingDevicesCommand = new RelayCommand(() => OpenControlPanel("recording"));
            OpenSoundsControlPanelCommand = new RelayCommand(() => OpenControlPanel("sounds"));
            OpenLegacyVolumeMixerCommand = new RelayCommand(() => Process.Start("sndvol.exe"));
            OpenEarTrumpetVolumeMixerCommand = new RelayCommand(FullWindow.ActivateSingleInstance);
            ChangeDeviceCommand = new RelayCommand<DeviceViewModel>((device) => device.MakeDefaultPlaybackDevice());
            StartAppServiceAndFeedbackHubCommand = new RelayCommand(FeedbackService.StartAppServiceAndFeedbackHub);
            OpenFlyoutCommand = new RelayCommand(_mainViewModel.OpenFlyout);
            ExitCommand = new RelayCommand(App.Current.Shutdown);
        }

        void UpdateTrayIcon()
        {
            int volume = _deviceManager.VirtualDefaultDevice.Volume.ToVolumeInt();
            IconId desiredIcon = IconId.OriginalIcon;

            if (!_deviceManager.VirtualDefaultDevice.IsDevicePresent)
            {
                desiredIcon = IconId.NoDevice;
            }
            else if (_deviceManager.VirtualDefaultDevice.IsMuted)
            {
                desiredIcon = IconId.Muted;
            }
            else if (volume == 0)
            {
                desiredIcon = IconId.SpeakerZeroBars;
            }
            else if (volume >= 1 && volume < 33)
            {
                desiredIcon = IconId.SpeakerOneBar;
            }
            else if (volume >= 33 && volume < 66)
            {
                desiredIcon = IconId.SpeakerTwoBars;
            }
            else if (volume >= 66 && volume <= 100)
            {
                desiredIcon = IconId.SpeakerThreeBars;
            }

            if (desiredIcon != _currentIcon)
            {
                _currentIcon = desiredIcon;
                TrayIcon = _icons[_currentIcon];
                RaisePropertyChanged(nameof(TrayIcon));
            }
        }

        private void OpenControlPanel(string panel)
        {
            Process.Start("rundll32.exe", $"shell32.dll,Control_RunDLL mmsys.cpl,,{panel}");
        }
    }
}
