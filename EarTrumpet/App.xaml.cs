using EarTrumpet.DataModel;
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
        private MainViewModel _viewModel;
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

            var deviceManager = DataModelFactory.CreateAudioDeviceManager();
            DiagnosticsService.Advise(deviceManager);

            _viewModel = new MainViewModel(deviceManager);
            _viewModel.Ready += MainViewModel_Ready;

            _flyoutWindow = new FlyoutWindow(_viewModel, new FlyoutViewModel(_viewModel));
            _trayIcon = new TrayIcon(new TrayViewModel(_viewModel));

            HotkeyService.Register(SettingsService.Hotkey);
            HotkeyService.KeyPressed += (_, __) => _viewModel.OpenFlyout(FlyoutShowOptions.Keyboard);

            StartupUWPDialogDisplayService.ShowIfAppropriate();

            Trace.WriteLine($"App Application_Startup Exit");
        }

        private void MainViewModel_Ready(object sender, System.EventArgs e)
        {
            Trace.WriteLine("App Application_Startup MainViewModel_Ready");
            _trayIcon.Show();
        }
    }
}
