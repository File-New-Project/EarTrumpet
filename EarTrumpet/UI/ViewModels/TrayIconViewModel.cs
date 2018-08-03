using EarTrumpet.Extensibility;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace EarTrumpet.UI.ViewModels
{
    public class TrayViewModel : BindableBase, ITrayViewModel
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

        internal static IAddonContextMenu[] AddonItems { get; set; }

        public event Action ContextMenuRequested;

        public Icon TrayIcon
        {
            get => _trayIcon;
            set
            {
                if (_trayIcon != value)
                {
                    _trayIcon = value;
                    RaisePropertyChanged(nameof(TrayIcon));
                }
            }
        }

        public string ToolTip
        {
            get => _toolTip;
            set
            {
                if (_toolTip != value)
                {
                    _toolTip = value;
                    RaisePropertyChanged(nameof(ToolTip));
                }
            }
        }

        public RelayCommand RightClick { get; }
        public RelayCommand MiddleClick { get; }
        public RelayCommand LeftClick { get; set; }
        public RelayCommand OpenMixer { get; set; }
        public RelayCommand OpenSettings { get; set; }

        private readonly string _trayIconPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\SndVolSSO.dll");
        private readonly DeviceCollectionViewModel _mainViewModel;
        private readonly Dictionary<IconId, Icon> _icons = new Dictionary<IconId, Icon>();
        private DeviceViewModel _defaultDevice;
        private bool _useLegacyIcon;
        private Icon _trayIcon;
        private string _toolTip;

        internal TrayViewModel(DeviceCollectionViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            RightClick = new RelayCommand(() => ContextMenuRequested?.Invoke());
            MiddleClick = new RelayCommand(ToggleMute);

            LoadIconResources();

            _useLegacyIcon = SettingsService.UseLegacyIcon;
            SettingsService.UseLegacyIconChanged += SettingsService_UseLegacyIconChanged;

            _mainViewModel.DefaultChanged += DeviceManager_DefaultDeviceChanged;
            DeviceManager_DefaultDeviceChanged(this, null);
        }

        public IEnumerable<ContextMenuItem> MenuItems
        {
            get
            {
                var ret = new List<ContextMenuItem>(_mainViewModel.AllDevices.OrderBy(x => x.DisplayName).Select(dev => new ContextMenuItem
                {
                    DisplayName = dev.DisplayName,
                    IsChecked = dev.Id == _defaultDevice?.Id,
                    Command = new RelayCommand(() => dev.MakeDefaultDevice()),
                }));

                if (!ret.Any())
                {
                    ret.Add(new ContextMenuItem
                    {
                        DisplayName = Properties.Resources.ContextMenuNoDevices,
                        IsEnabled = false,
                    });
                }
                else
                {
                    ret.Add(new ContextMenuSeparator { });
                }

                ret.AddRange(new List<ContextMenuItem>
                {
                    new ContextMenuItem{ DisplayName = Properties.Resources.FullWindowTitleText,   Command =  OpenMixer },
                    new ContextMenuItem{ DisplayName = Properties.Resources.LegacyVolumeMixerText, Command =  new RelayCommand(StartLegacyMixer) },
                    new ContextMenuSeparator{ },
                    new ContextMenuItem{ DisplayName = Properties.Resources.PlaybackDevicesText,    Command = new RelayCommand(() => OpenControlPanel("playback")) },
                    new ContextMenuItem{ DisplayName = Properties.Resources.RecordingDevicesText,   Command = new RelayCommand(() => OpenControlPanel("recording")) },
                    new ContextMenuItem{ DisplayName = Properties.Resources.SoundsControlPanelText, Command = new RelayCommand(() => OpenControlPanel("sounds")) },
                    new ContextMenuSeparator{ },
                    new ContextMenuItem{ DisplayName = Properties.Resources.SettingsWindowText,     Command = OpenSettings },
                    new ContextMenuItem{ DisplayName = Properties.Resources.ContextMenuSendFeedback,Command = new RelayCommand(FeedbackService.OpenFeedbackHub) },
                    new ContextMenuItem{ DisplayName = Properties.Resources.ContextMenuExitTitle,   Command = new RelayCommand(App.Current.Shutdown) },
                });

                if (Features.IsEnabled(Feature.Addons))
                {
                    var addonEntries = new List<ContextMenuItem>();
                    if (AddonItems.SelectMany(a => a.Items).Any())
                    {
                        // Add a separator before and after each extension group.
                        foreach (var ext in AddonItems.OrderBy(x => x.Items.FirstOrDefault()?.DisplayName))
                        {
                            addonEntries.Add(new ContextMenuSeparator());

                            foreach (var item in ext.Items)
                            {
                                addonEntries.Add(item);
                            }

                            addonEntries.Add(new ContextMenuSeparator());
                        }

                        // Remove duplicate separators (extensions may also add separators)
                        bool previousItemWasSeparator = false;
                        for (var i = addonEntries.Count - 1; i >= 0; i--)
                        {
                            var currentItemIsSeparator = addonEntries[i] is ContextMenuSeparator;

                            if ((i == addonEntries.Count - 1 || i == 0) && currentItemIsSeparator)
                            {
                                addonEntries.Remove(addonEntries[i]);
                            }

                            if (previousItemWasSeparator && currentItemIsSeparator)
                            {
                                addonEntries.Remove(addonEntries[i]);
                            }

                            previousItemWasSeparator = currentItemIsSeparator;
                        }
                    }

                    if (addonEntries.Any())
                    {
                        ret.Insert(ret.Count - 3, new ContextMenuItem
                        {
                            DisplayName = Properties.Resources.AddonsMenuText,
                            Children = addonEntries,
                        });
                        ret.Insert(ret.Count - 3, new ContextMenuSeparator { });
                    }
                }

                return ret;
            }
        }

        public void DpiChanged()
        {
            Trace.WriteLine("TrayViewModel DpiChanged");

            LoadIconResources();
            UpdateTrayIcon();
        }

        private void LoadIconResources()
        {
            _icons.Clear();
            var useLargeIcon = WindowsTaskbar.Current.Dpi > 1;
            Trace.WriteLine($"TrayViewModel LoadIconResources useLargeIcon={useLargeIcon}");

            var originalIcon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/EarTrumpet;component/Assets/Tray.ico")).Stream);
            try
            {
                _icons.Add(IconId.OriginalIcon, originalIcon);
                _icons.Add(IconId.NoDevice, IconUtils.GetIconFromFile(_trayIconPath, (int)IconId.NoDevice, useLargeIcon));
                _icons.Add(IconId.Muted, IconUtils.GetIconFromFile(_trayIconPath, (int)IconId.Muted, useLargeIcon));
                _icons.Add(IconId.SpeakerOneBar, IconUtils.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerOneBar, useLargeIcon));
                _icons.Add(IconId.SpeakerTwoBars, IconUtils.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerTwoBars, useLargeIcon));
                _icons.Add(IconId.SpeakerThreeBars, IconUtils.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerThreeBars, useLargeIcon));
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{ex}");

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

        private void DeviceManager_DefaultDeviceChanged(object sender, DeviceViewModel newDefaultDevice)
        {
            if (_defaultDevice != null)
            {
                _defaultDevice.PropertyChanged -= DefaultDevice_PropertyChanged;
            }

            _defaultDevice = newDefaultDevice;

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

        private void SettingsService_UseLegacyIconChanged(object sender, bool newSetting)
        {
            _useLegacyIcon = newSetting;
            UpdateTrayIcon();
        }

        private void UpdateTrayIcon()
        {
            if (_useLegacyIcon)
            {
                TrayIcon = _icons[IconId.OriginalIcon];
            }
            else if (_defaultDevice == null)
            {
                TrayIcon = _icons[IconId.NoDevice];
            }
            else
            {
                var iconKind = _defaultDevice.IconKind;
                switch(iconKind)
                {
                    case DeviceIconKind.Mute:
                        TrayIcon = _icons[IconId.Muted];
                        break;
                    case DeviceIconKind.Bar1:
                        TrayIcon = _icons[IconId.SpeakerOneBar];
                        break;
                    case DeviceIconKind.Bar2:
                        TrayIcon = _icons[IconId.SpeakerTwoBars];
                        break;
                    case DeviceIconKind.Bar3:
                        TrayIcon = _icons[IconId.SpeakerThreeBars];
                        break;
                    default: throw new NotImplementedException();
                }
            }
        }

        private void ToggleMute()
        {
            if (_defaultDevice != null)
            {
                _defaultDevice.IsMuted = !_defaultDevice.IsMuted;
            }
        }

        private void UpdateToolTip()
        {
            if (_defaultDevice != null)
            {
                var otherText = "EarTrumpet: 100% - ";
                var dev = _defaultDevice.DisplayName;
                // API Limitation: "less than 64 chars" for the tooltip.
                dev = dev.Substring(0, Math.Min(63 - otherText.Length, dev.Length));
                ToolTip = $"EarTrumpet: {_defaultDevice.Volume}% - {dev}";
            }
            else
            {
                ToolTip = Properties.Resources.NoDeviceTrayText;
            }
        }

        private void OpenControlPanel(string panel)
        {
            try
            {
                using (Process.Start("rundll32.exe", $"shell32.dll,Control_RunDLL mmsys.cpl,,{panel}"))
                { }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{ex}");
            }
        }

        private void StartLegacyMixer()
        {
            try
            {
                var pos = System.Windows.Forms.Cursor.Position;
                using (Process.Start("sndvol.exe", $"-a {User32.MAKEWPARAM((ushort)pos.X, (ushort)pos.Y)}"))
                { }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{ex}");
            }
        }
    }
}