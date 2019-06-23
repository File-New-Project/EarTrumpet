using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.Diagnosis;
using EarTrumpet.Extensibility.Hosting;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.UI.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet
{
    public partial class App
    {
        public static readonly string AssetBaseUri = "pack://application:,,,/EarTrumpet;component/Assets/";
        public static bool IsShuttingDown { get; private set; }
        public static TimeSpan Duration => s_appTimer.Elapsed;

        public FlyoutViewModel FlyoutViewModel { get; private set; }
        public FlyoutWindow FlyoutWindow { get; private set; }
        public DeviceCollectionViewModel PlaybackDevicesViewModel { get; private set; }

        private static readonly Stopwatch s_appTimer = Stopwatch.StartNew();
        private ShellNotifyIcon _trayIcon;
        private WindowHolder _mixerWindow;
        private WindowHolder _settingsWindow;
        private ErrorReporter _errorReporter;
        private AppSettings _settings;

        private void OnAppStartup(object sender, StartupEventArgs e)
        {
            Exit += (_, __) => IsShuttingDown = true;
            _errorReporter = new ErrorReporter();

            if (SingleInstanceAppMutex.TakeExclusivity())
            {
                Exit += (_, __) => SingleInstanceAppMutex.ReleaseExclusivity();

                try
                {
                    ContinueStartup();
                }
                catch (Exception ex) when (IsCriticalFontLoadFailure(ex))
                {
                    ErrorReporter.LogWarning(ex);
                    OnCriticalFontLoadFailure();
                }
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
            _mixerWindow = new WindowHolder(CreateMixerExperience);
            _settingsWindow = new WindowHolder(CreateSettingsExperience);
            PlaybackDevicesViewModel = new DeviceCollectionViewModel(WindowsAudioFactory.Create(AudioDeviceKind.Playback), _settings);
            PlaybackDevicesViewModel.Ready += (_, __) => CompleteStartup();

            _trayIcon = new ShellNotifyIcon(
                new TaskbarIconSource(PlaybackDevicesViewModel, _settings), 
                () => _settings.TrayIconIdentity, 
                _settings.ResetTrayIconIdentity);
            PlaybackDevicesViewModel.TrayPropertyChanged += () => _trayIcon.SetTooltip(PlaybackDevicesViewModel.GetTrayToolTip());
            Exit += (_, __) => _trayIcon.IsVisible = false;

            FlyoutViewModel = new FlyoutViewModel(PlaybackDevicesViewModel, () => _trayIcon.SetFocus());
            FlyoutWindow = new FlyoutWindow(FlyoutViewModel);

            // Initialize the FlyoutWindow last because its Show/Hide cycle will pump messages, causing UI frames
            // to be executed, breaking the assumption that startup is complete.
            FlyoutWindow.Initialize();
        }

        private void CompleteStartup()
        {
            AddonManager.Load();
            Exit += (_, __) => AddonManager.Shutdown();

            _trayIcon.PrimaryInvoke += (_, type) => FlyoutViewModel.OpenFlyout(type);
            _trayIcon.SecondaryInvoke += (_, __) => _trayIcon.ShowContextMenu(GetTrayContextMenuItems());
            _trayIcon.TertiaryInvoke += (_, __) => PlaybackDevicesViewModel.Default?.ToggleMute.Execute(null);
            _trayIcon.Scrolled += (_, wheelDelta) => PlaybackDevicesViewModel.Default?.IncrementVolume(Math.Sign(wheelDelta) * 2);
            _settings.FlyoutHotkeyTyped += () => FlyoutViewModel.OpenFlyout(InputType.Keyboard);
            _settings.MixerHotkeyTyped += () => _mixerWindow.OpenOrClose();
            _settings.SettingsHotkeyTyped += () => _settingsWindow.OpenOrBringToFront();

            _trayIcon.IsVisible = true;

            DisplayFirstRunExperience();
        }

        private void DisplayFirstRunExperience()
        {
            if (!_settings.HasShownFirstRun)
            {
                Trace.WriteLine($"App DisplayFirstRunExperience Showing welcome dialog");
                _settings.HasShownFirstRun = true;

                var viewModel = new WelcomeViewModel();
                var dialog = new DialogWindow { DataContext = viewModel };
                viewModel.Close = new RelayCommand(() => dialog.Close());
                dialog.Show();
            }
        }

        private bool IsCriticalFontLoadFailure(Exception ex)
        {
            return ex.StackTrace.Contains("MS.Internal.Text.TextInterface.FontFamily.GetFirstMatchingFont") ||
                   ex.StackTrace.Contains("MS.Internal.Text.Line.Format");
        }

        private void OnCriticalFontLoadFailure()
        {
            Trace.WriteLine($"App OnCriticalFontLoadFailure");

            new Thread(() =>
            {
                if (MessageBox.Show(
                    EarTrumpet.Properties.Resources.CriticalFailureFontLookupHelpText,
                    EarTrumpet.Properties.Resources.CriticalFailureDialogHeaderText,
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK) == MessageBoxResult.OK)
                {
                    Trace.WriteLine($"App OnCriticalFontLoadFailure OK");
                    ProcessHelper.StartNoThrow("https://eartrumpet.app/jmp/fixfonts");
                }
                Environment.Exit(0);
            }).Start();

            // Stop execution because callbacks to the UI thread will likely cause another cascading font error.
            new AutoResetEvent(false).WaitOne();
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

            var addonItems = AddonManager.Host.TrayContextMenuItems?.OrderBy(x => x.Items.FirstOrDefault()?.DisplayName).SelectMany(ext => ext.Items);
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
            window.Closed += (_, __) => _mixerWindow.Destroyed();
            return window;
        }

        private Window CreateSettingsExperience()
        {
            var defaultCategory = new SettingsCategoryViewModel(
                EarTrumpet.Properties.Resources.SettingsCategoryTitle,
                "\xE71D",
                EarTrumpet.Properties.Resources.SettingsDescriptionText,
                null,
                new SettingsPageViewModel[]
                    {
                        new EarTrumpetShortcutsPageViewModel(_settings),
                        new EarTrumpetLegacySettingsPageViewModel(_settings),
                        new EarTrumpetAboutPageViewModel(() => _errorReporter.DisplayDiagnosticData())
                    });

            var allCategories = new List<SettingsCategoryViewModel>();
            allCategories.Add(defaultCategory);

            if (AddonManager.Host.SettingsItems != null)
            {
                allCategories.AddRange(AddonManager.Host.SettingsItems.Select(a => a.Get(AddonManager.FindAddonInfoForObject(a))));
            }

            var viewModel = new SettingsViewModel(EarTrumpet.Properties.Resources.SettingsWindowText, allCategories);
            var window = new SettingsWindow { DataContext = viewModel };
            window.Closed += (_, __) => _settingsWindow.Destroyed();
            viewModel.Close = new RelayCommand(() => window.Close());
            return window;
        }
    }
}