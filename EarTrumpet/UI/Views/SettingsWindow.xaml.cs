using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using EarTrumpet.UI.ViewModels;
using System.Diagnostics;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class SettingsWindow : Window
    {
        private static SettingsWindow Instance;

        private SettingsViewModel _viewModel;

        internal SettingsWindow()
        {
            Trace.WriteLine("SettingsWindow .ctor");
            Instance = this;

            InitializeComponent();

            Title = Properties.Resources.SettingsWindowText;
            _viewModel = new SettingsViewModel();
            DataContext = _viewModel;

            Closing += SettingsWindow_Closing;
            SourceInitialized += SettingsWindow_SourceInitialized;

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        private void SettingsWindow_SourceInitialized(object sender, System.EventArgs e)
        {
            Trace.WriteLine("SettingsWindow SettingsWindow_SourceInitialized");

            this.Cloak();
            AccentPolicyLibrary.SetWindowBlur(this, true, true);
        }

        private void SettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Trace.WriteLine("SettingsWindow SettingsWindow_Closing");

            Instance = null;
        }

        public static void ActivateSingleInstance()
        {
            Trace.WriteLine("SettingsWindow ActivateSingleInstance");

            if (Instance == null)
            {
                var window = new SettingsWindow();
                window.Show();
                WindowAnimationLibrary.BeginWindowEntranceAnimation(window, () => { });
            }
            else
            {
                Instance.RaiseWindow();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("SettingsWindow CloseButton_Click");

            WindowAnimationLibrary.BeginWindowExitAnimation(this, () => this.Close());
        }

        private void HotkeySelect_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("SettingsWindow HotkeySelect_Click");

            HotkeyService.Unregister();

            var win = new HotkeySelectionWindow();
            win.Owner = this;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if ((bool)win.ShowDialog())
            {
                _viewModel.Hotkey = win.Hotkey;
            }

            HotkeyService.Register(_viewModel.Hotkey);
        }
    }
}
