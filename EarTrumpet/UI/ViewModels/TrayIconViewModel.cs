using EarTrumpet.Interop;
using EarTrumpet.Properties;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using EarTrumpet.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;

namespace EarTrumpet.UI.ViewModels
{
    public class TrayViewModel : BindableBase
    {
        public enum IconId
        {
            Invalid = 0,
            Muted = 120,
            SpeakerZeroBars = 121,
            SpeakerOneBar = 122,
            SpeakerTwoBars = 123,
            SpeakerThreeBars = 124,
            NoDevice = 125,
            OriginalIcon
        }

        public Icon TrayIcon { get; private set; }
        public string ToolTip { get; private set; }
        public string DefaultDeviceId => _defaultDevice?.Id;
        public RelayCommand OpenSettingsCommand { get; }
        public RelayCommand OpenPlaybackDevicesCommand { get; }
        public RelayCommand OpenRecordingDevicesCommand { get; }
        public RelayCommand OpenSoundsControlPanelCommand { get; }
        public RelayCommand OpenLegacyVolumeMixerCommand { get; }
        public RelayCommand OpenEarTrumpetVolumeMixerCommand { get; }
        public RelayCommand<DeviceViewModel> ChangeDeviceCommand { get; }
        public RelayCommand OpenFeedbackHubCommand { get; }
        public RelayCommand OpenFlyoutCommand { get; }
        public RelayCommand ExitCommand { get; }
        public IEnumerable<Tuple<string, RelayCommand>>[] StaticCommands { get; }

        private readonly string _trayIconPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\SndVolSSO.dll");
        private readonly DeviceCollectionViewModel _mainViewModel;
        private readonly Dictionary<IconId, Icon> _icons = new Dictionary<IconId, Icon>();
        private IconId _currentIcon = IconId.Invalid;
        private bool _useLegacyIcon;
        private DeviceViewModel _defaultDevice;

        public ObservableCollection<DeviceViewModel> AllDevices => _mainViewModel.AllDevices;

        internal TrayViewModel(DeviceCollectionViewModel mainViewModel, Action openFlyout)
        {
            _mainViewModel = mainViewModel;

            LoadIconResources();

            _useLegacyIcon = SettingsService.UseLegacyIcon;
            SettingsService.UseLegacyIconChanged += SettingsService_UseLegacyIconChanged;

            _mainViewModel.DefaultChanged += DeviceManager_DefaultDeviceChanged;
            DeviceManager_DefaultDeviceChanged(this, null);

            OpenSettingsCommand = new RelayCommand(SettingsWindow.ActivateSingleInstance);
            OpenPlaybackDevicesCommand = new RelayCommand(() => OpenControlPanel("playback"));
            OpenRecordingDevicesCommand = new RelayCommand(() => OpenControlPanel("recording"));
            OpenSoundsControlPanelCommand = new RelayCommand(() => OpenControlPanel("sounds"));
            OpenLegacyVolumeMixerCommand = new RelayCommand(StartLegacyMixer);
            OpenEarTrumpetVolumeMixerCommand = new RelayCommand(() => FullWindow.ActivateSingleInstance(_mainViewModel));
            ChangeDeviceCommand = new RelayCommand<DeviceViewModel>((device) => device.MakeDefaultPlaybackDevice());
            OpenFeedbackHubCommand = new RelayCommand(FeedbackService.OpenFeedbackHub);
            OpenFlyoutCommand = new RelayCommand(openFlyout);
            ExitCommand = new RelayCommand(App.Current.Shutdown);

            var staticCommands = new List<List<Tuple<string, RelayCommand>>>();
            staticCommands.Add(new List<Tuple<string, RelayCommand>>()
            {
                new Tuple<string,RelayCommand>(Resources.FullWindowTitleText, OpenEarTrumpetVolumeMixerCommand),
                new Tuple<string,RelayCommand>(Resources.LegacyVolumeMixerText, OpenLegacyVolumeMixerCommand),
            });
            staticCommands.Add(new List<Tuple<string, RelayCommand>>()
            {
                new Tuple<string,RelayCommand>(Resources.PlaybackDevicesText, OpenPlaybackDevicesCommand),
                new Tuple<string,RelayCommand>(Resources.RecordingDevicesText, OpenRecordingDevicesCommand),
                new Tuple<string,RelayCommand>(Resources.SoundsControlPanelText, OpenSoundsControlPanelCommand),
            });
            staticCommands.Add(new List<Tuple<string, RelayCommand>>()
            {
                new Tuple<string,RelayCommand>(Resources.SettingsWindowText, OpenSettingsCommand),
                new Tuple<string,RelayCommand>(Resources.ContextMenuSendFeedback, OpenFeedbackHubCommand),
                new Tuple<string,RelayCommand>(Resources.ContextMenuExitTitle, ExitCommand),
            });
            StaticCommands = staticCommands.ToArray();
        }

        private void LoadIconResources()
        {
            var originalIcon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/EarTrumpet;component/Assets/Tray.ico")).Stream);
            try
            {
                _icons.Add(IconId.OriginalIcon, originalIcon);
                _icons.Add(IconId.NoDevice, GetIconFromFile(_trayIconPath, (int)IconId.NoDevice));
                _icons.Add(IconId.Muted, GetIconFromFile(_trayIconPath, (int)IconId.Muted));
                _icons.Add(IconId.SpeakerZeroBars, GetIconFromFile(_trayIconPath, (int)IconId.SpeakerZeroBars));
                _icons.Add(IconId.SpeakerOneBar, GetIconFromFile(_trayIconPath, (int)IconId.SpeakerOneBar));
                _icons.Add(IconId.SpeakerTwoBars, GetIconFromFile(_trayIconPath, (int)IconId.SpeakerTwoBars));
                _icons.Add(IconId.SpeakerThreeBars, GetIconFromFile(_trayIconPath, (int)IconId.SpeakerThreeBars));
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");

                _icons.Clear();
                _icons.Add(IconId.OriginalIcon, originalIcon);
                _icons.Add(IconId.NoDevice, originalIcon);
                _icons.Add(IconId.Muted, originalIcon);
                _icons.Add(IconId.SpeakerZeroBars, originalIcon);
                _icons.Add(IconId.SpeakerOneBar, originalIcon);
                _icons.Add(IconId.SpeakerTwoBars, originalIcon);
                _icons.Add(IconId.SpeakerThreeBars, originalIcon);
            }
        }

        private void DeviceManager_DefaultDeviceChanged(object sender, DeviceViewModel e)
        {
            if (_defaultDevice != null)
            {
                _defaultDevice.PropertyChanged -= DefaultDevice_PropertyChanged;
            }

            _defaultDevice = e;

            if (_defaultDevice != null)
            {
                _defaultDevice.PropertyChanged += DefaultDevice_PropertyChanged;
            }

            DefaultDevice_PropertyChanged(sender, null);
        }

        private void DefaultDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateTrayIcon();
            UpdateToolTip();
        }

        private void SettingsService_UseLegacyIconChanged(object sender, bool e)
        {
            _useLegacyIcon = e;
            UpdateTrayIcon();
        }

        private void UpdateTrayIcon()
        {
            IconId desiredIcon = IconId.OriginalIcon;

            if (_useLegacyIcon)
            {
                desiredIcon = IconId.OriginalIcon;
            }
            else
            {
                int volume = _defaultDevice != null ? _defaultDevice.Volume : 0;

                if (_defaultDevice == null)
                {
                    desiredIcon = IconId.NoDevice;
                }
                else if (_defaultDevice.IsMuted)
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
            }

            if (desiredIcon != _currentIcon)
            {
                _currentIcon = desiredIcon;
                TrayIcon = _icons[_currentIcon];
                RaisePropertyChanged(nameof(TrayIcon));
            }
        }

        internal void ToggleMute()
        {
            if (_defaultDevice != null)
            {
                _defaultDevice.IsMuted = !_defaultDevice.IsMuted;
            }
        }

        private void UpdateToolTip()
        {
            string toolTipText;
            if (_defaultDevice != null)
            {
                var otherText = "EarTrumpet: 100% - ";
                var dev = _defaultDevice.DisplayName;
                // API Limitation: "less than 64 chars" for the tooltip.
                dev = dev.Substring(0, Math.Min(63 - otherText.Length, dev.Length));
                toolTipText = $"EarTrumpet: {_defaultDevice.Volume}% - {dev}";
            }
            else
            {
                toolTipText = Resources.NoDeviceTrayText;
            }

            if (toolTipText != ToolTip)
            {
                ToolTip = toolTipText;
                RaisePropertyChanged(nameof(ToolTip));
            }
        }

        private void OpenControlPanel(string panel)
        {
            using (Process.Start("rundll32.exe", $"shell32.dll,Control_RunDLL mmsys.cpl,,{panel}")) { }
        }

        private void StartLegacyMixer()
        {
            ProcessHelper.StartNoThrow("sndvol.exe");
        }

        private static Icon GetIconFromFile(string path, int iconOrdinal = 0)
        {
            var moduleHandle = Kernel32.LoadLibraryEx(path, IntPtr.Zero,
                Kernel32.LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE | Kernel32.LoadLibraryFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE);

            IntPtr iconHandle = IntPtr.Zero;
            try
            {
                Comctl32.LoadIconMetric(moduleHandle, new IntPtr(iconOrdinal), Comctl32.LI_METRIC.LIM_SMALL, ref iconHandle);
            }
            finally
            {
                Kernel32.FreeLibrary(moduleHandle);
            }

            return Icon.FromHandle(iconHandle);
        }
    }
}
