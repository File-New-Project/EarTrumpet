using EarTrumpet.DataModel;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using EarTrumpet.Views;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet
{
    public partial class App
    {
        private FlyoutWindow _flyoutWindow;
        private MainViewModel _viewModel;
        private IAudioDeviceManager _deviceManager;
        private TrayViewModel _trayViewModel;
        private TrayIcon _trayIcon;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!SingleInstanceAppMutex.TakeExclusivity())
            {
                Current.Shutdown();
                return;
            }

            Exit += (_, __) => SingleInstanceAppMutex.ReleaseExclusivity();

            WhatsNewDisplayService.ShowIfAppropriate();
            FirstRunDisplayService.ShowIfAppropriate();

            _deviceManager = DataModelFactory.CreateAudioDeviceManager();
            _viewModel = new MainViewModel(_deviceManager);

            _flyoutWindow = new FlyoutWindow(_viewModel, _deviceManager, (ThemeService)Resources["ThemeService"]);

            _trayViewModel = new TrayViewModel(_deviceManager);
            _trayIcon = new TrayIcon(_deviceManager, _trayViewModel);
            _trayIcon.Invoked += () => _flyoutWindow.OpenAsFlyout();

            HotkeyService.Register(SettingsService.Hotkey);

#if VSDEBUG
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                new DebugWindow().Show();
            }
#endif
        }
    }
}
