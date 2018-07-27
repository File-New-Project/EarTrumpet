using EarTrumpet.Extensibility;
using EarTrumpet.Extensions;
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

        public event Action ContextMenuRequested;

        public Icon TrayIcon { get; private set; }
        public string ToolTip { get; private set; }
        public RelayCommand RightClick { get; }
        public RelayCommand MiddleClick { get; }
        public RelayCommand LeftClick { get; }
        public IAddonContextMenu[] AddonItems { get; set; }

        private readonly string _trayIconPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\SndVolSSO.dll");
        private readonly DeviceCollectionViewModel _mainViewModel;
        private readonly Dictionary<IconId, Icon> _icons = new Dictionary<IconId, Icon>();
        private IconId _currentIcon = IconId.Invalid;
        private bool _useLegacyIcon;
        private bool _useLargeIcon;
        private DeviceViewModel _defaultDevice;
        private SettingsWindow _openSettingsWindow;

        internal TrayViewModel(DeviceCollectionViewModel mainViewModel, Action openFlyout)
        {
            _mainViewModel = mainViewModel;

            LeftClick = new RelayCommand(openFlyout);
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
                        DisplayName = Resources.ContextMenuNoDevices,
                        IsEnabled = false,
                    });
                }
                else
                {
                    ret.Add(new ContextMenuSeparator { });
                }

                ret.AddRange(new List<ContextMenuItem>
                {
                    new ContextMenuItem{ DisplayName =Resources.FullWindowTitleText,   Command =  new RelayCommand(() => FullWindow.ActivateSingleInstance(_mainViewModel)) },
                    new ContextMenuItem{ DisplayName =Resources.LegacyVolumeMixerText, Command =  new RelayCommand(StartLegacyMixer) },
                    new ContextMenuSeparator{ },
                    new ContextMenuItem{ DisplayName = Resources.PlaybackDevicesText,    Command = new RelayCommand(() => OpenControlPanel("playback")) },
                    new ContextMenuItem{ DisplayName = Resources.RecordingDevicesText,   Command = new RelayCommand(() => OpenControlPanel("recording")) },
                    new ContextMenuItem{ DisplayName = Resources.SoundsControlPanelText, Command = new RelayCommand(() => OpenControlPanel("sounds")) },
                    new ContextMenuSeparator{ },
                    new ContextMenuItem{ DisplayName = Resources.SettingsWindowText,     Command = new RelayCommand(OpenSettings) },
                    new ContextMenuItem{ DisplayName = Resources.ContextMenuSendFeedback,Command = new RelayCommand(FeedbackService.OpenFeedbackHub) },
                    new ContextMenuItem{ DisplayName = Resources.ContextMenuExitTitle,   Command = new RelayCommand(App.Current.Shutdown) },
                });

                var addonEntries = new List<ContextMenuItem>();
                if (AddonItems.SelectMany(a => a.Items).Any())
                {
                    // Add a line before and after each extension group.
                    foreach (var ext in AddonItems.OrderBy(x => x.Items.FirstOrDefault()?.DisplayName))
                    {
                        addonEntries.Add(new ContextMenuSeparator());

                        foreach (var item in ext.Items)
                        {
                            addonEntries.Add(item);
                        }

                        addonEntries.Add(new ContextMenuSeparator());
                    }

                    // Remove duplicate lines (extensions may also add lines)
                    bool prevItemWasSep = false;
                    for (var i = addonEntries.Count - 1; i >= 0; i--)
                    {
                        var itemIsSep = addonEntries[i] is ContextMenuSeparator;

                        if ((i == addonEntries.Count - 1 || i == 0) && itemIsSep)
                        {
                            addonEntries.Remove(addonEntries[i]);
                        }

                        if (prevItemWasSep && itemIsSep)
                        {
                            addonEntries.Remove(addonEntries[i]);
                        }

                        prevItemWasSep = itemIsSep;
                    }
                }

                if (addonEntries.Any())
                {
                    ret.Insert(ret.Count - 3, new ContextMenuItem
                    {
                        DisplayName = "Addons",
                        Children = addonEntries,
                    });
                    ret.Insert(ret.Count - 3, new ContextMenuSeparator { });
                }

                return ret;
            }
        }

        private void OpenSettings()
        {
            if (_openSettingsWindow != null)
            {
                _openSettingsWindow.RaiseWindow();
            }
            else
            {
                var viewModel = new SettingsViewModel();
                viewModel.RequestHotkey += ViewModel_RequestHotkey;
                viewModel.OpenAddonManager = new RelayCommand(() =>
                {
                    var window = new DialogWindow { Owner = _openSettingsWindow };
                    var addonManagerViewModel = new AddonManagerViewModel(Extensibility.Hosting.AddonManager.Current);
                    addonManagerViewModel.Added += (a) =>
                    {
                        var paths = Extensibility.Hosting.AddonManager.Current.AdditionalPaths.ToList();
                        paths.Add(a.DisplayName);
                        Extensibility.Hosting.AddonManager.Current.AdditionalPaths = paths.ToArray();
                    };
                    addonManagerViewModel.Removed += (a) =>
                    {
                        var paths = Extensibility.Hosting.AddonManager.Current.AdditionalPaths.ToList();
                        paths.Remove(paths.First(p => p.ToLower() == a.DisplayName.ToLower()));
                        Extensibility.Hosting.AddonManager.Current.AdditionalPaths = paths.ToArray();
                    };
                    window.DataContext = addonManagerViewModel;
                    window.ShowDialog();
                });
                _openSettingsWindow = new SettingsWindow();
                _openSettingsWindow.DataContext = viewModel;
                _openSettingsWindow.Closing += (_, __) => _openSettingsWindow = null;
                _openSettingsWindow.Show();
                WindowAnimationLibrary.BeginWindowEntranceAnimation(_openSettingsWindow, () => { });
            }
        }

        private HotkeyData ViewModel_RequestHotkey(HotkeyData currentHotkey)
        {
            Trace.WriteLine("TrayViewModel ViewModel_RequestHotkey");

            bool userSaved = false;
            var window = new DialogWindow { Owner = _openSettingsWindow };
            var viewModel = new HotkeySelectViewModel
            {
                Save = new RelayCommand(() =>
                {
                    userSaved = true;
                    window.Close();
                })
            };
            window.DataContext = viewModel;
            window.PreviewKeyDown += viewModel.Window_PreviewKeyDown;
            window.ShowDialog();

            if (userSaved)
            {
                return viewModel.Hotkey;
            }
            return currentHotkey;
        }

        private void LoadIconResources()
        {
            _useLargeIcon = WindowsTaskbar.Current.Dpi > 1;
            Trace.WriteLine($"TrayViewModel LoadIconResources useLargeIcon={_useLargeIcon}");

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

        private void ToggleMute()
        {
            if (_defaultDevice != null)
            {
                _defaultDevice.IsMuted = !_defaultDevice.IsMuted;
            }
        }

        internal void DpiChanged()
        {
            Trace.WriteLine("TrayViewModel DpiChanged");
            _icons.Clear();
            LoadIconResources();
            _currentIcon = IconId.Invalid;
            UpdateTrayIcon();
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

        private Icon GetIconFromFile(string path, int iconOrdinal = 0)
        {
            var moduleHandle = Kernel32.LoadLibraryEx(path, IntPtr.Zero,
                Kernel32.LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE | Kernel32.LoadLibraryFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE);

            IntPtr iconHandle = IntPtr.Zero;
            try
            {
                Comctl32.LoadIconMetric(moduleHandle, new IntPtr(iconOrdinal), _useLargeIcon ? Comctl32.LI_METRIC.LIM_LARGE : Comctl32.LI_METRIC.LIM_SMALL, ref iconHandle);
            }
            finally
            {
                Kernel32.FreeLibrary(moduleHandle);
            }

            return Icon.FromHandle(iconHandle);
        }
    }
}