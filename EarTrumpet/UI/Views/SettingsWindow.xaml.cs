using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using System;
using System.Diagnostics;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class SettingsWindow : Window
    {
        public event Action CloseClicked;

        private bool _isClosing;

        public SettingsWindow()
        {
            Trace.WriteLine("SettingsWindow .ctor");

            InitializeComponent();

            SourceInitialized += SettingsWindow_SourceInitialized;
            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            Themes.Manager.Current.ThemeChanged += SetBlurColor;
            Closed += (_, __) => Themes.Manager.Current.ThemeChanged -= SetBlurColor;
            StateChanged += OnWindowStateChanged;
        }

        private void SetBlurColor()
        {
            AccentPolicyLibrary.EnableAcrylic(this, Themes.Manager.Current.ResolveRef(this, "AcrylicColor_Settings"), Interop.User32.AccentFlags.DrawAllBorders);
        }

        private void SettingsWindow_SourceInitialized(object sender, System.EventArgs e)
        {
            Trace.WriteLine("SettingsWindow SettingsWindow_SourceInitialized");

            this.Cloak();
            SetBlurColor();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("SettingsWindow CloseButton_Click SafeClose");
            e.Handled = true;

            CloseClicked?.Invoke();
        }

        public void SafeClose()
        {
            Trace.WriteLine("SettingsWindow SafeClose");

            if (!_isClosing)
            {
                // Ensure we don't double-animate if the user is able to close us multiple ways before the window stops accepting input.
                _isClosing = true;
                WindowAnimationLibrary.BeginWindowExitAnimation(this, () => this.Close());
            }
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            var chrome = System.Windows.Shell.WindowChrome.GetWindowChrome(this);
            chrome.ResizeBorderThickness = WindowState == WindowState.Maximized ? new Thickness(0) : SystemParameters.WindowResizeBorderThickness;

            if (WindowState == WindowState.Maximized)
            {
                UpdateLayout();
                WindowSizeHelper.RestrictMaximizedSizeToWorkArea(this);
            }
        }
    }
}
