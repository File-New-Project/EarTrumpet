using EarTrumpet.DataModel;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Controls;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using EarTrumpet.UI.Themes;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.UI.Views;
using System.Diagnostics;
using System.Windows;

namespace EarTrumpet
{
    public partial class App
    {
        public FlyoutViewModel FlyoutViewModel { get; private set; }
        public TrayViewModel TrayViewModel { get; private set; }

        private DeviceCollectionViewModel _viewModel;
        private TrayIcon _trayIcon;
        private FlyoutWindow _flyoutWindow;

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

            _viewModel = new DeviceCollectionViewModel(DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback));
            _viewModel.Ready += MainViewModel_Ready;

            FlyoutViewModel = new FlyoutViewModel(_viewModel);
            _flyoutWindow = new FlyoutWindow(FlyoutViewModel);
            TrayViewModel = new TrayViewModel(_viewModel, () => FlyoutViewModel.OpenFlyout(FlyoutShowOptions.Pointer));
            _trayIcon = new TrayIcon(TrayViewModel);
            _flyoutWindow.DpiChanged += (_, __) => TrayViewModel.DpiChanged();

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
            Trace.WriteLine("App Application_Startup MainViewModel_Ready");
            _trayIcon.Show();

            Extensibility.Hosting.AddonManager.Current.Load();
        }
    }
}