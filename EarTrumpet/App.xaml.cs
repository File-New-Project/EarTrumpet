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
        public static bool IsShuttingDown { get; private set; }
        public static bool HasIdentity { get; private set; }
        public static Version PackageVersion { get; private set; }
        public static TimeSpan Duration => s_appTimer.Elapsed;

        private static readonly Stopwatch s_appTimer = Stopwatch.StartNew();
        private FlyoutViewModel _flyoutViewModel;
        private FlyoutWindow _flyoutWindow;
        private DeviceCollectionViewModel _collectionViewModel;
        private ShellNotifyIcon _trayIcon;
        private WindowHolder _mixerWindow;
        private WindowHolder _settingsWindow;
        private ErrorReporter _errorReporter;
        private AppSettings _settings;

        private void OnAppStartup(object sender, StartupEventArgs e)
        {
            Exit += (_, __) => IsShuttingDown = true;
            HasIdentity = PackageHelper.CheckHasIdentity();
            PackageVersion = PackageHelper.GetVersion(HasIdentity);
            _settings = new AppSettings();
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

            var deviceManager = WindowsAudioFactory.Create(AudioDeviceKind.Playback);
            deviceManager.Loaded += (_, __) => CompleteStartup();
            _collectionViewModel = new DeviceCollectionViewModel(deviceManager, _settings);

            _trayIcon = new ShellNotifyIcon(
                new TaskbarIconSource(_collectionViewModel, _settings), () => _settings.TrayIconIdentity, _settings.ResetTrayIconIdentity);
            Exit += (_, __) => _trayIcon.IsVisible = false;
            _collectionViewModel.TrayPropertyChanged += () => _trayIcon.SetTooltip(_collectionViewModel.GetTrayToolTip());

            _flyoutViewModel = new FlyoutViewModel(_collectionViewModel, () => _trayIcon.SetFocus());
            _flyoutWindow = new FlyoutWindow(_flyoutViewModel);
            // Initialize the FlyoutWindow last because its Show/Hide cycle will pump messages, causing UI frames
            // to be executed, breaking the assumption that startup is complete.
            _flyoutWindow.Initialize();
        }

        private void CompleteStartup()
        {
            AddonManager.Load();
            Exit += (_, __) => AddonManager.Shutdown();

#if DEBUG
            DebugHelpers.Add();
#endif
            _mixerWindow = new WindowHolder(CreateMixerExperience);
            _settingsWindow = new WindowHolder(CreateSettingsExperience);

            _settings.FlyoutHotkeyTyped += () => _flyoutViewModel.OpenFlyout(InputType.Keyboard);
            _settings.MixerHotkeyTyped += () => _mixerWindow.OpenOrClose();
            _settings.SettingsHotkeyTyped += () => _settingsWindow.OpenOrBringToFront();
            _settings.RegisterHotkeys();

            _trayIcon.PrimaryInvoke += (_, type) => _flyoutViewModel.OpenFlyout(type);
            _trayIcon.SecondaryInvoke += (_, __) => _trayIcon.ShowContextMenu(GetTrayContextMenuItems());
            _trayIcon.TertiaryInvoke += (_, __) => _collectionViewModel.Default?.ToggleMute.Execute(null);
            _trayIcon.Scrolled += (_, wheelDelta) => _collectionViewModel.Default?.IncrementVolume(Math.Sign(wheelDelta) * 2);
            _trayIcon.SetTooltip(_collectionViewModel.GetTrayToolTip());
            _trayIcon.IsVisible = true;

            DisplayFirstRunExperience();
        }

        private void DisplayFirstRunExperience()
        {
            if (!_settings.HasShownFirstRun
#if DEBUG
                || Keyboard.IsKeyDown(Key.LeftCtrl)
#endif
                )
            {
                Trace.WriteLine($"App DisplayFirstRunExperience Showing welcome dialog");
                _settings.HasShownFirstRun = true;

                var dialog = new DialogWindow { DataContext = new WelcomeViewModel() };
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
            var ret = new List<ContextMenuItem>(_collectionViewModel.AllDevices.OrderBy(x => x.DisplayName).Select(dev => new ContextMenuItem
            {
                DisplayName = dev.DisplayName,
                IsChecked = dev.Id == _collectionViewModel.Default?.Id,
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
            return new SettingsWindow { DataContext = viewModel };
        }

        private Window CreateMixerExperience() => new FullWindow { DataContext = new FullWindowViewModel(_collectionViewModel) };
    }
}