using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using System;
using System.Diagnostics;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            Trace.WriteLine("SettingsWindow .ctor");
            Closed += (_, __) => Trace.WriteLine("SettingsWindow Closed");

            InitializeComponent();

            Themes.Manager.Current.ThemeChanged += EnableAcrylic;
            SourceInitialized += OnSourceInitialized;
            StateChanged += OnWindowStateChanged;
        }

        private void EnableAcrylic()
        {
            AccentPolicyLibrary.EnableAcrylic(this, Themes.Manager.Current.ResolveRef(this, "AcrylicColor_Settings"), Interop.User32.AccentFlags.DrawAllBorders);
        }

        private void OnSourceInitialized(object sender, System.EventArgs e)
        {
            this.Cloak();
            EnableAcrylic();
        }

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            var chrome = System.Windows.Shell.WindowChrome.GetWindowChrome(this);
            chrome.ResizeBorderThickness = WindowState == WindowState.Maximized ? new Thickness(0) : SystemParameters.WindowResizeBorderThickness;

            if (WindowState == WindowState.Maximized)
            {
                WindowSizeHelper.RestrictMaximizedSizeToWorkArea(this);
            }
        }
    }
}
