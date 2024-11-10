using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.UI.ViewManagement;

namespace EarTrumpet.UI.Themes;

public sealed class Manager : BindableBase, INotifyPropertyChanged, IDisposable
{
    public static Manager Current { get; private set; }

    public event Action ThemeChanged;

    public List<Ref> References { get; }

    private bool? lastAnimationsEnabledValue;
    public bool AnimationsEnabled {
        get
        {
            if (!lastAnimationsEnabledValue.HasValue)
            {
                if (Environment.OSVersion.IsAtLeast(OSVersions.Windows11))
                {
                    lastAnimationsEnabledValue = new UISettings().AnimationsEnabled;
                }
                else
                {
                    // Windows 10 taskbar flyouts are incorrectly tied to [SPI_GETANIMATION]
                    // ANIMATIONINFO.iMinAnimate
                    lastAnimationsEnabledValue = SystemParameters.MinimizeAnimation;
                }
            }
            return lastAnimationsEnabledValue.Value;
        }
    }
    public static bool IsLightTheme => SystemSettings.IsLightTheme;
    public static bool IsSystemLightTheme => SystemSettings.IsSystemLightTheme;
    public static bool IsHighContrast => SystemParameters.HighContrast;
    public static bool UseAccentColorOnWindowBorders => SystemSettings.UseAccentColorOnWindowBorders;
    public static bool UseDynamicScrollbars => SystemSettings.UseDynamicScrollbars;

    private DispatcherTimer _themeChangeTimer = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private Win32Window _messageWindow;

    public Manager()
    {
        Current = this;
        References = [];
        _themeChangeTimer.Tick += ThemeChangeTimer_Tick;

        _messageWindow = new Win32Window();
        _messageWindow.Initialize((m) => WndProc(m.Msg, m.WParam, m.LParam));
    }

    ~Manager()
    {
        _themeChangeTimer.Tick -= ThemeChangeTimer_Tick;
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public void Load()
    {
        // This method needs to be called from App to get us loaded otherwise XAML will lazy-load us.
    }

    public static Color ResolveRef(DependencyObject target, string key)
    {
        return BrushValueParser.Parse(target, key).Color;
    }

    private void WndProc(int msg, IntPtr _, IntPtr lParam)
    {
        const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x320;
        const int WM_DWMCOMPOSITIONCHANGED = 0x31E;
        const int WM_THEMECHANGED = 0x031A;
        const int WM_SETTINGCHANGE = 0x001A;

        switch (msg)
        {
            case WM_DWMCOLORIZATIONCOLORCHANGED:
            case WM_DWMCOMPOSITIONCHANGED:
            case WM_THEMECHANGED:
                OnThemeColorsChanged();
                break;
            case WM_SETTINGCHANGE:
                var settingChanged = Marshal.PtrToStringUni(lParam);
                if (settingChanged == "ImmersiveColorSet" || // Accent color
                    settingChanged == "WindowsThemeElement") // High contrast
                {
                    OnThemeColorsChanged();
                }
                else if (settingChanged == "WindowMetrics")
                {
                    lastAnimationsEnabledValue = null;
                    RaisePropertyChanged(nameof(AnimationsEnabled));
                }
                break;
        }
    }

    private void OnThemeColorsChanged()
    {
        if (_themeChangeTimer.IsEnabled)
        {
            _themeChangeTimer.Stop();
        }
        _themeChangeTimer.Start();
    }

    private void ThemeChangeTimer_Tick(object sender, EventArgs e)
    {
        _themeChangeTimer.IsEnabled = false;

        ThemeChanged?.Invoke();
        RaisePropertyChanged(nameof(IsLightTheme));
        RaisePropertyChanged(nameof(IsSystemLightTheme));
        RaisePropertyChanged(nameof(IsHighContrast));
        RaisePropertyChanged(nameof(UseAccentColorOnWindowBorders));
    }

    public void Dispose()
    {
        _messageWindow.Dispose();
        GC.SuppressFinalize(this);
    }
}