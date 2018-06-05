using EarTrumpet.DataModel;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using EarTrumpet.Views;
using System;
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
            Bugsnag.Clients.WPFClient.Start();

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new AppTraceListener());

            var watch = Stopwatch.StartNew();
            Trace.WriteLine("App Application_Startup");

            if (!SingleInstanceAppMutex.TakeExclusivity())
            {
                Trace.WriteLine("App Application_Startup TakeExclusivity failed");
                Current.Shutdown();
                return;
            }

            Exit += (_, __) => SingleInstanceAppMutex.ReleaseExclusivity();

            ((ThemeManager)Resources["ThemeManager"]).SetTheme(ThemeData.GetBrushData());

            var deviceManager = DataModelFactory.CreateAudioDeviceManager();
            deviceManager.PlaybackDevicesLoaded += DeviceManager_PlaybackDevicesLoaded;
            DiagnosticsService.Advise(deviceManager);

            _viewModel = new MainViewModel(deviceManager);
            _flyoutWindow = new FlyoutWindow(_viewModel, new FlyoutViewModel(_viewModel, deviceManager));
            _trayIcon = new TrayIcon(new TrayViewModel(_viewModel, deviceManager));

            HotkeyService.Register(SettingsService.Hotkey);
            HotkeyService.KeyPressed += (_, __) => _viewModel.OpenFlyout();

            StartupUWPDialogDisplayService.ShowIfAppropriate();

#if VSDEBUG
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                new DebugWindow().Show();
            }
#endif
            Trace.WriteLine($"App Application_Startup time= {watch.ElapsedMilliseconds} ms");
        }

        private void DeviceManager_PlaybackDevicesLoaded(object sender, System.EventArgs e)
        {
            Trace.WriteLine("App Application_Startup DeviceManager_PlaybackDevicesLoaded");

            _trayIcon.Show();
        }
    }
}
