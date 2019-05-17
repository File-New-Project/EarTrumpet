using EarTrumpet.DataModel;
using EarTrumpet.Diagnosis;
using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Hosting;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.UI.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet
{
    public partial class App
    {
        public static readonly string AssetBaseUri = "pack://application:,,,/EarTrumpet;component/Assets/";
        internal static IAddonContextMenu[] AddonTrayContextMenuItems { get; set; }

        public FlyoutViewModel FlyoutViewModel { get; private set; }
        public FlyoutWindow FlyoutWindow { get; private set; }
        public DeviceCollectionViewModel PlaybackDevicesViewModel { get; private set; }
        public bool IsShuttingDown { get; private set; }

        private ShellNotifyIcon _trayIcon;
        private WindowHolder _mixerWindow;
        private WindowHolder _settingsWindow;
        private ErrorReporter _errorReporter;
        private AddonManager _addonManager;
        private AppSettings _settings;

        private void OnAppStartup(object sender, StartupEventArgs e)
        {
            Exit += (_, __) => IsShuttingDown = true;
            _errorReporter = new ErrorReporter();

            if (SingleInstanceAppMutex.TakeExclusivity())
            {
                Exit += (_, __) => SingleInstanceAppMutex.ReleaseExclusivity();
                ContinueStartup();
            }
            else
            {
                Shutdown();
            }
        }

        private void ContinueStartup()
        {
            ((UI.Themes.Manager)Resources["ThemeManager"]).Load();

            _settings = new AppSettings();
            _settings.FlyoutHotkeyTyped += () => FlyoutViewModel.OpenFlyout(InputType.Keyboard);
            _settings.MixerHotkeyTyped += () => _mixerWindow.OpenOrClose();
            _settings.SettingsHotkeyTyped += () => _settingsWindow.OpenOrBringToFront();
            _mixerWindow = new WindowHolder(CreateMixerExperience);
            _settingsWindow = new WindowHolder(CreateSettingsExperience);
            PlaybackDevicesViewModel = new DeviceCollectionViewModel(DataModel.WindowsAudio.WindowsAudioFactory.Create(DataModel.WindowsAudio.AudioDeviceKind.Playback), _settings);
            PlaybackDevicesViewModel.Ready += (_, __) => CompleteStartup();
            PlaybackDevicesViewModel.TrayPropertyChanged += () => UpdateTrayTooltipAndIcon();
            FlyoutViewModel = new FlyoutViewModel(PlaybackDevicesViewModel, () => _trayIcon.SetFocus());
            FlyoutWindow = new FlyoutWindow(FlyoutViewModel);

            CreateTrayExperience();
        }

        private void CreateTrayExperience()
        {
            if (!SndVolSSO.SystemIconsAreAvailable())
            {
                _settings.UseLegacyIcon = true;
            }

            TaskbarIconSource iconSource = null;
            iconSource = new TaskbarIconSource(icon =>
            {
                if (_settings.UseLegacyIcon)
                {
                    icon.Dispose();
                    icon = IconHelper.LoadIconForTaskbar(SystemSettings.IsSystemLightTheme ? $"{AssetBaseUri}Application.ico" : $"{AssetBaseUri}Tray.ico");
                }

                double iconFillPercent = ((SndVolSSO.IconId)iconSource.Tag) == SndVolSSO.IconId.NoDevice && !_settings.UseLegacyIcon ? 0.4 : 1;
                if (SystemParameters.HighContrast)
                {
                    icon = IconHelper.ColorIcon(icon, iconFillPercent, _trayIcon.IsMouseOver ? SystemColors.HighlightTextColor : SystemColors.WindowTextColor);
                }
                else if (SystemSettings.IsSystemLightTheme && !_settings.UseLegacyIcon)
                {
                    icon = IconHelper.ColorIcon(icon, iconFillPercent, System.Windows.Media.Colors.Black);
                }
                return icon;
            },
            () => $"hc={SystemParameters.HighContrast} {(SystemParameters.HighContrast ? $"mouse={_trayIcon.IsMouseOver}" : "")} dpi={WindowsTaskbar.Dpi} theme={SystemSettings.IsSystemLightTheme} legacy={_settings.UseLegacyIcon}");
            _settings.UseLegacyIconChanged += (_, __) => iconSource.CheckForUpdate();

            _trayIcon = new ShellNotifyIcon(iconSource, () => _settings.TrayIconIdentity, _settings.ResetTrayIconIdentity);
            _trayIcon.PrimaryInvoke += (_, type) => FlyoutViewModel.OpenFlyout(type);
            _trayIcon.SecondaryInvoke += (_, __) => _trayIcon.ShowContextMenu(GetTrayContextMenuItems());
            _trayIcon.TertiaryInvoke += (_, __) => PlaybackDevicesViewModel.Default?.ToggleMute.Execute(null);
            _trayIcon.Scrolled += (_, wheelDelta) => PlaybackDevicesViewModel.Default?.IncrementVolume(Math.Sign(wheelDelta) * 2);
            Exit += (_, __) => _trayIcon.IsVisible = false;

            UpdateTrayTooltipAndIcon();
        }

        private void CompleteStartup()
        {
            _addonManager = new AddonManager();
            Exit += (_, __) => _addonManager.Shutdown();

            _trayIcon.IsVisible = true;
            DisplayFirstRunExperience();
        }

        private void UpdateTrayTooltipAndIcon()
        {
            var iconType = (PlaybackDevicesViewModel.Default == null) ? SndVolSSO.IconId.NoDevice : PlaybackDevicesViewModel.Default.GetSndVolIcon();
            _trayIcon.IconSource.Tag = iconType;
            _trayIcon.IconSource.Source = SndVolSSO.GetPath(iconType);
            _trayIcon.SetTooltip(PlaybackDevicesViewModel.GetTrayToolTip());
        }

        private void DisplayFirstRunExperience()
        {
            if (!_settings.HasShownFirstRun)
            {
                Trace.WriteLine($"App DisplayFirstRunExperience Showing welcome dialog");
                _settings.HasShownFirstRun = true;

                var viewModel = new WelcomeViewModel();
                var dialog = new DialogWindow { DataContext = viewModel };
                viewModel.Close = new RelayCommand(() => dialog.SafeClose());
                dialog.Show();
            }
        }

        private IEnumerable<ContextMenuItem> GetTrayContextMenuItems()
        {
            var ret = new List<ContextMenuItem>(PlaybackDevicesViewModel.AllDevices.OrderBy(x => x.DisplayName).Select(dev => new ContextMenuItem
            {
                DisplayName = dev.DisplayName,
                IsChecked = dev.Id == PlaybackDevicesViewModel.Default?.Id,
                Command = new RelayCommand(() => dev.MakeDefaultDevice()),
            }));

            if (!ret.Any())
            {
                ret.Add(new ContextMenuItem
                {
                    DisplayName = EarTrumpet.Properties.Resources.ContextMenuNoDevices,
                    IsEnabled = false,
                });
            }

            ret.AddRange(new List<ContextMenuItem>
                {
                    new ContextMenuSeparator(),
                    new ContextMenuItem
                    {
                        DisplayName = EarTrumpet.Properties.Resources.WindowsLegacyMenuText,
                        Children = new List<ContextMenuItem>
                        {
                            new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.LegacyVolumeMixerText, Command =  new RelayCommand(LegacyControlPanelHelper.StartLegacyAudioMixer) },
                            new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.PlaybackDevicesText, Command = new RelayCommand(() => LegacyControlPanelHelper.Open("playback")) },
                            new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.RecordingDevicesText, Command = new RelayCommand(() => LegacyControlPanelHelper.Open("recording")) },
                            new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.SoundsControlPanelText, Command = new RelayCommand(() => LegacyControlPanelHelper.Open("sounds")) },
                            new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.OpenSoundSettingsText, Command = new RelayCommand(() => LegacyControlPanelHelper.Open("ms-settings:sound")) },
                        },
                    },
                    new ContextMenuSeparator(),
                });

            var addonItems = AddonTrayContextMenuItems?.OrderBy(x => x.Items.FirstOrDefault()?.DisplayName).SelectMany(ext => ext.Items);
            if (addonItems != null && addonItems.Any())
            {
                ret.AddRange(addonItems);
                ret.Add(new ContextMenuSeparator());
            }

            ret.AddRange(new List<ContextMenuItem>
                {
                    new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.FullWindowTitleText, Command = new RelayCommand(_mixerWindow.OpenOrBringToFront) },
                    new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.SettingsWindowText, Command = new RelayCommand(_settingsWindow.OpenOrBringToFront) },
                    new ContextMenuItem { DisplayName = EarTrumpet.Properties.Resources.ContextMenuExitTitle, Command = new RelayCommand(Shutdown) },
                });
            return ret;
        }

        private Window CreateMixerExperience()
        {
            var viewModel = new FullWindowViewModel(PlaybackDevicesViewModel);
            var window = new FullWindow { DataContext = viewModel };
            window.Closing += (_, __) =>
            {
                _mixerWindow.Destroyed();
                viewModel.Close();
            };
            return window;
        }

        private Window CreateSettingsExperience()
        {
            var defaultCategory = new SettingsCategoryViewModel(
                EarTrumpet.Properties.Resources.SettingsCategoryTitle,
                "\xE71D",
                EarTrumpet.Properties.Resources.SettingsDescriptionText,
                null,
                new SettingsPageViewModel[] {
                        new EarTrumpetShortcutsPageViewModel(_settings),
                        new EarTrumpetLegacySettingsPageViewModel(_settings),
                        new EarTrumpetAboutPageViewModel(() => _errorReporter.DisplayDiagnosticData(string.Join(" ", _addonManager.All.Select(a => a.DisplayName))))
                }.ToList());

            var allCategories = new List<SettingsCategoryViewModel>();
            allCategories.Add(defaultCategory);
            if (SettingsViewModel.AddonItems != null)
            {
                allCategories.AddRange(SettingsViewModel.AddonItems.Select(a => a.Get(_addonManager.FindAddonForObject(a))));
            }

            bool canClose = false;
            var viewModel = new SettingsViewModel(EarTrumpet.Properties.Resources.SettingsWindowText, allCategories);
            var window = new SettingsWindow { DataContext = viewModel };
            window.CloseClicked += () => viewModel.OnClosing();
            viewModel.Close += () =>
            {
                canClose = true;
                window.SafeClose();
            };
            window.Closing += (_, e) =>
            {
                if (canClose)
                {
                    _settingsWindow.Destroyed();
                }
                else
                {
                    e.Cancel = true;
                    viewModel.OnClosing();
                }
            };
            return window;
        }
    }
}