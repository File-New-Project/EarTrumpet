using EarTrumpet.Extensibility;
using EarTrumpet.Interop;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace EarTrumpet.UI.Tray
{
    public class TrayViewModel : BindableBase, ITrayViewModel
    {
        internal static IAddonContextMenu[] AddonItems { get; set; }

        private Icon _trayIcon;
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

        private string _toolTip;
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

        public ICommand MiddleClick { get; }
        public ICommand LeftClick { get; set; }
        public ICommand OpenMixer { get; set; }
        public ICommand OpenSettings { get; set; }

        protected readonly DeviceCollectionViewModel _mainViewModel;
        protected DeviceViewModel _defaultDevice;

        public TrayViewModel(DeviceCollectionViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            MiddleClick = new RelayCommand(ToggleMute);
            _mainViewModel.DefaultChanged += DeviceManager_DefaultDeviceChanged;
            DeviceManager_DefaultDeviceChanged(this, _mainViewModel.Default);
            Themes.Manager.Current.PropertyChanged += (_, e) => UpdateTrayIcon();
        }

        public virtual IEnumerable<ContextMenuItem> MenuItems
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

                var legacyItems = new List<ContextMenuItem>
                {
                    new ContextMenuItem{ DisplayName = Properties.Resources.LegacyVolumeMixerText, Command =  new RelayCommand(StartLegacyMixer) },
                    new ContextMenuItem{ DisplayName = Properties.Resources.PlaybackDevicesText,    Command = new RelayCommand(() => OpenControlPanel("playback")) },
                    new ContextMenuItem{ DisplayName = Properties.Resources.RecordingDevicesText,   Command = new RelayCommand(() => OpenControlPanel("recording")) },
                    new ContextMenuItem{ DisplayName = Properties.Resources.SoundsControlPanelText, Command = new RelayCommand(() => OpenControlPanel("sounds")) },
                    new ContextMenuItem{ DisplayName = Properties.Resources.OpenSoundSettingsText, Command = new RelayCommand(() => OpenControlPanel("ms-settings:sound")) },
                };
                var legacyMenu = new ContextMenuItem
                {
                    DisplayName = Properties.Resources.WindowsLegacyMenuText,
                    Children = legacyItems,
                };

                ret.AddRange(new List<ContextMenuItem>
                {
                    legacyMenu,
                    new ContextMenuItem{ DisplayName = Properties.Resources.FullWindowTitleText,   Command =  OpenMixer },
                    new ContextMenuItem{ DisplayName = Properties.Resources.SettingsWindowText, Command = OpenSettings },
                    new ContextMenuItem{ DisplayName = Properties.Resources.ContextMenuExitTitle,   Command = new RelayCommand(App.Current.Shutdown) },
                });

                var addonEntries = new List<ContextMenuItem>();
                if (AddonItems != null && AddonItems.SelectMany(a => a.Items).Any())
                {
                    foreach (var ext in AddonItems.OrderBy(x => x.Items.FirstOrDefault()?.DisplayName))
                    {
                        // Add a separator before and after each extension group.
                        addonEntries.Add(new ContextMenuSeparator());
                        addonEntries.AddRange(ext.Items);
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
                    foreach (var entry in addonEntries)
                    {
                        ret.Insert(ret.Count - 4, entry);
                    }
                    ret.Insert(ret.Count - 4, new ContextMenuSeparator { });
                }

                return ret;
            }
        }

        public void Refresh()
        {
            Trace.WriteLine("TrayViewModel Refresh");
            UpdateTrayIcon();
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
            if (e == null || e.PropertyName == nameof(_defaultDevice.Volume) || e.PropertyName == nameof(_defaultDevice.IsMuted) || e.PropertyName == nameof(_defaultDevice.DisplayName))
            {
                UpdateTrayIcon();
                UpdateToolTip();
            }
        }

        protected virtual void UpdateTrayIcon()
        {
            if (_defaultDevice == null)
            {
                TrayIcon = TrayIconFactory.CreateAndResolveAll(IconKind.NoDevice);
            }
            else
            {
                var iconKind = _defaultDevice.IconKind;
                switch (iconKind)
                {
                    case DeviceIconKind.Mute:
                        TrayIcon = TrayIconFactory.CreateAndResolveAll(IconKind.Muted);
                        break;
                    case DeviceIconKind.Bar1:
                        TrayIcon = TrayIconFactory.CreateAndResolveAll(IconKind.SpeakerOneBar);
                        break;
                    case DeviceIconKind.Bar2:
                        TrayIcon = TrayIconFactory.CreateAndResolveAll(IconKind.SpeakerTwoBars);
                        break;
                    case DeviceIconKind.Bar3:
                        TrayIcon = TrayIconFactory.CreateAndResolveAll(IconKind.SpeakerThreeBars);
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

        protected virtual void UpdateToolTip()
        {
            if (_defaultDevice != null)
            {
                var stateText = _defaultDevice.IsMuted ? Properties.Resources.MutedText : $"{_defaultDevice.Volume}%";
                var prefixText = $"EarTrumpet: {stateText} - ";
                var deviceName = $"{_defaultDevice.DeviceDescription} ({_defaultDevice.EnumeratorName})";

                // Note: Remote Desktop has an empty description and empty enumerator, but the friendly name is set.
                if (string.IsNullOrWhiteSpace(_defaultDevice.DeviceDescription) && string.IsNullOrWhiteSpace(_defaultDevice.EnumeratorName))
                {
                    deviceName = _defaultDevice.DisplayName;
                }

                // API Limitation: "less than 64 chars" for the tooltip.
                deviceName = deviceName.Substring(0, Math.Min(63 - prefixText.Length, deviceName.Length));
                ToolTip = prefixText + deviceName;
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
                var rundllPath = Path.Combine(
                    Environment.GetEnvironmentVariable("SystemRoot"),
                    (Environment.Is64BitOperatingSystem ? @"sysnative\rundll32.exe" : @"system32\rundll32.exe"));

                using (Process.Start(rundllPath, $"shell32.dll,Control_RunDLL mmsys.cpl,,{panel}"))
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