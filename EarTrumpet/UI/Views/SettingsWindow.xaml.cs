using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class SettingsWindow : Window
    {
        private static Dictionary<ISettingsViewModel, SettingsWindow> s_windows = new Dictionary<ISettingsViewModel, SettingsWindow>();

        private ISettingsViewModel _viewModel;
        private bool _isClosing;

        internal SettingsWindow(ISettingsViewModel viewModel)
        {
            Trace.WriteLine("SettingsWindow .ctor");
            s_windows.Add(viewModel, this);

            InitializeComponent();

            _viewModel = viewModel;
            _viewModel.RequestHotkey += OnRequestHotkey;

            ComponentHost.Content = _viewModel;
            DataContext = _viewModel;

            Closing += SettingsWindow_Closing;
            SourceInitialized += SettingsWindow_SourceInitialized;

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        private HotkeyData OnRequestHotkey(HotkeyData arg)
        {
            Trace.WriteLine("SettingsWindow HotkeySelect_Click");

            var win = new HotkeySelectionWindow { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            return win.ShowDialog() == true ? win.Hotkey : null;
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

            s_windows.Remove(_viewModel);
        }

        public static void ActivateSingleInstance(ISettingsViewModel viewModel)
        {
            Trace.WriteLine("SettingsWindow ActivateSingleInstance");

            if (!s_windows.TryGetValue(viewModel, out var instance))
            {
                var window = new SettingsWindow(viewModel);
                window.Show();
                WindowAnimationLibrary.BeginWindowEntranceAnimation(window, () => { });
            }
            else
            {
                instance.RaiseWindow();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("SettingsWindow CloseButton_Click");

            if (!_isClosing)
            {
                // Ensure we don't double-animate if the user is able to close us multiple ways before the window stops accepting input.
                _isClosing = true;
                WindowAnimationLibrary.BeginWindowExitAnimation(this, () => this.Close());
            }
        }
    }
}
