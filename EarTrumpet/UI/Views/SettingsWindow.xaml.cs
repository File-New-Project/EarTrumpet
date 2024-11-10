using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace EarTrumpet.UI.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        Trace.WriteLine("SettingsWindow .ctor");
        Closed += (_, __) => Trace.WriteLine("SettingsWindow Closed");

        InitializeComponent();

        SourceInitialized += (sender, __) =>
        {
            this.Cloak();
            this.EnableRoundedCornersIfApplicable();

            if (App.Settings.SettingsWindowPlacement != null)
            {
                User32.SetWindowPlacement(new WindowInteropHelper((Window)sender).Handle, App.Settings.SettingsWindowPlacement.Value);
            }
        };

        StateChanged += OnWindowStateChanged;

        Closing += (sender, __) =>
        {
            if (User32.GetWindowPlacement(new WindowInteropHelper((Window)sender).Handle, out var placement))
            {
                App.Settings.SettingsWindowPlacement = placement;
            }
        };
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
