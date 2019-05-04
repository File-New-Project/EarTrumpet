using EarTrumpet.DataModel;
using EarTrumpet.Interop.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace EarTrumpet.UI.Themes
{
    public class Manager : BindableBase, INotifyPropertyChanged
    {
        public static Manager Current { get; private set; }

        public event Action ThemeChanged;

        public List<Ref> References { get; }
        public bool AnimationsEnabled => SystemParameters.MenuAnimation;
        public bool IsLightTheme => SystemSettings.IsLightTheme;
        public bool IsSystemLightTheme => SystemSettings.IsSystemLightTheme;
        public bool IsHighContrast => SystemParameters.HighContrast;
        public bool UseAccentColorOnWindowBorders => SystemSettings.UseAccentColorOnWindowBorders;
        public bool UseDynamicScrollbars => SystemSettings.UseDynamicScrollbars;

        private DispatcherTimer _themeChangeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        private Win32Window _messageWindow;

        public Manager()
        {
            Current = this;
            References = new List<Ref>();
            _themeChangeTimer.Tick += ThemeChangeTimer_Tick;

            _messageWindow = new Win32Window();
            _messageWindow.Initialize((m) => WndProc(m.Msg, m.WParam, m.LParam));
        }

        ~Manager()
        {
            _themeChangeTimer.Tick -= ThemeChangeTimer_Tick;
        }

        public void Load()
        {
            // This method needs to be called from App to get us loaded otherwise XAML will lazy-load us.
        }

        public Color ResolveRef(DependencyObject target, string key)
        {
            return BrushValueParser.Parse(target, key).Color;
        }

        private void WndProc(int msg, IntPtr wParam, IntPtr lParam)
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
    }
}