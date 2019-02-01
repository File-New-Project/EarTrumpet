using EarTrumpet.DataModel;
using EarTrumpet.Extensibility.Hosting;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Controls;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using EarTrumpet.UI.Themes;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.UI.Views;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace EarTrumpet
{
    public partial class App
    {
        public FlyoutViewModel FlyoutViewModel { get; private set; }
        public TrayViewModel TrayViewModel { get; private set; }
        public FlyoutWindow FlyoutWindow { get; private set; }
        public DeviceCollectionViewModel PlaybackDevicesViewModel { get; private set; }

        private TrayIcon _trayIcon;
        private SettingsWindow _openSettingsWindow;
        private FullWindow _openMixerWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ErrorReportingService.Initialize();

            Trace.WriteLine("App Application_Startup");

            if (!SingleInstanceAppMutex.TakeExclusivity())
            {
                Trace.WriteLine("App Application_Startup TakeExclusivity failed");
                Current.Shutdown();
                return;
            }

            ((ThemeManager)Resources["ThemeManager"]).SetTheme(ThemeData.GetBrushData());

            PlaybackDevicesViewModel = new DeviceCollectionViewModel(DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback));
            PlaybackDevicesViewModel.Ready += MainViewModel_Ready;

            FlyoutViewModel = new FlyoutViewModel(PlaybackDevicesViewModel);
            FlyoutWindow = new FlyoutWindow(FlyoutViewModel);

            TrayViewModel = new TrayViewModel(PlaybackDevicesViewModel);
            TrayViewModel.LeftClick = new RelayCommand(() => FlyoutViewModel.OpenFlyout(FlyoutShowOptions.Pointer));
            TrayViewModel.OpenMixer = new RelayCommand(OpenMixer);
            TrayViewModel.OpenSettings = new RelayCommand(OpenSettings);

            _trayIcon = new TrayIcon(TrayViewModel);
            FlyoutWindow.DpiChanged += (_, __) => TrayViewModel.DpiChanged();

            HotkeyManager.Current.Register(SettingsService.Hotkey);
            HotkeyManager.Current.KeyPressed += (hotkey) =>
            {
                if (hotkey.Equals(SettingsService.Hotkey))
                {
                    FlyoutViewModel.OpenFlyout(FlyoutShowOptions.Keyboard);
                }
            };

            StartupUWPDialogDisplayService.ShowIfAppropriate();

            Trace.WriteLine($"App Application_Startup Exit");
        }

        private void MainViewModel_Ready(object sender, System.EventArgs e)
        {
            Trace.WriteLine("App MainViewModel_Ready");
            _trayIcon.IsVisible = true;

            Trace.WriteLine("App MainViewModel_Ready Before Load");
            AddonManager.Current.Load();
            Trace.WriteLine("App MainViewModel_Ready After Load");


            // TODO: NOT FOR CHECKIN
            OpenSettings();

        }

        private void OpenMixer()
        {
            if (_openMixerWindow != null)
            {
                _openMixerWindow.RaiseWindow();
            }
            else
            {
                var viewModel = new FullWindowViewModel(PlaybackDevicesViewModel);
                _openMixerWindow = new FullWindow();
                _openMixerWindow.DataContext = viewModel;
                _openMixerWindow.Closing += (_, __) =>
                {
                    _openMixerWindow = null;
                    viewModel.Close();
                };
                _openMixerWindow.Show();
                WindowAnimationLibrary.BeginWindowEntranceAnimation(_openMixerWindow, () => { });
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
                /*
                var viewModel = new SettingsViewModel();
                viewModel.OpenAddonManager = new RelayCommand(() =>
                {
                    var window = new DialogWindow { Owner = _openSettingsWindow };
                    var addonManagerViewModel = new AddonManagerViewModel(AddonManager.Current);
                    window.DataContext = addonManagerViewModel;
                    window.ShowDialog();
                });
                */

                _openSettingsWindow = new SettingsWindow();
                
                var defaultCategory = new SettingsCategoryViewModel("Settings", "\xE115", "Settings, About, Help",
                    new SettingsPageViewModel[] {
                        new EarTrumpetLegacySettingsPageViewModel(),
                        new EarTrumpetAboutPageViewModel("Information")
                    }.ToList());

                var allCategories = new List<SettingsCategoryViewModel>();
                allCategories.Add(defaultCategory);
                if (SettingsViewModel.AddonItems != null)
                {
                    allCategories.AddRange(SettingsViewModel.AddonItems.Select(a => a.Get()));
                }
                var viewModel = new SettingsViewModel("Settings", allCategories);
                _openSettingsWindow.DataContext = viewModel;
                _openSettingsWindow.Closing += (_, __) => _openSettingsWindow = null;
                _openSettingsWindow.Show();
                WindowAnimationLibrary.BeginWindowEntranceAnimation(_openSettingsWindow, () => { });
            }
        }
    }
}