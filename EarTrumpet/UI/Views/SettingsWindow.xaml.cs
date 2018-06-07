using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using EarTrumpet.UI.ViewModels;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class SettingsWindow : Window
    {
        private static SettingsWindow Instance;

        private SettingsViewModel _viewModel;

        internal SettingsWindow()
        {
            InitializeComponent();

            Title = Properties.Resources.SettingsWindowText;
            _viewModel = new SettingsViewModel();
            DataContext = _viewModel;

            Instance = this;
            Closing += (s, e) =>
            {
                 Instance = null;
            };

            SourceInitialized += (_, __) =>
            {
                this.Cloak();
                AccentPolicyLibrary.SetWindowBlur(this, true, true);
            };

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        public static void ActivateSingleInstance()
        {
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
            WindowAnimationLibrary.BeginWindowExitAnimation(this, () => this.Close());
        }

        private void HotkeySelect_Click(object sender, RoutedEventArgs e)
        {
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
