using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace EarTrumpet.Services
{
    public class ThemeService
    {
        public static event Action ThemeChanged;

        public static bool IsWindowTransparencyEnabled
        {
            get { return !SystemParameters.HighContrast && UserSystemPreferencesService.IsTransparencyEnabled; }
        }

        public static void LoadCurrentTheme()
        {
            var newDictionary = new ResourceDictionary();
            var themeDictionary = Application.Current.Resources.MergedDictionaries[0];

            newDictionary["WindowForeground"] = Lookup("ImmersiveApplicationTextDarkTheme");
            newDictionary["HeaderBackground"] = Lookup("ImmersiveSystemAccent", 0.2);
            newDictionary["HeaderBackgroundSolid"] = Lookup("ImmersiveSystemAccent", 0.6);
            newDictionary["CottonSwabSliderThumb"] = Lookup("ImmersiveSystemAccent");
            newDictionary["CottonSwabSliderThumbHover"] = Lookup("ImmersiveControlDarkSliderThumbHover");
            newDictionary["CottonSwabSliderThumbPressed"] = Lookup("ImmersiveControlDarkSliderThumbHover");
            newDictionary["CottonSwabSliderTrackFill"] = Lookup("ImmersiveSystemAccentLight1");
            newDictionary["BadgeBackground"] = Lookup("ImmersiveSystemAccentDark2", 0.8);
            newDictionary["WindowBackground"] = new SolidColorBrush(GetWindowBackgroundColor());
            newDictionary["PeakMeterHotColor"] = Lookup(IsWindowTransparencyEnabled ? "ImmersiveSystemAccentDark2" : "ImmersiveSystemAccentDark3");

            Application.Current.Resources.MergedDictionaries.Remove(themeDictionary);
            Application.Current.Resources.MergedDictionaries.Insert(0, newDictionary);
        }

        public static void RegisterForThemeChanges(IntPtr hwnd)
        {
            var src = HwndSource.FromHwnd(hwnd);
            src.AddHook(WndProc);
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x320;
            const int WM_DWMCOMPOSITIONCHANGED = 0x31E;
            const int WM_THEMECHANGED = 0x031A;

            switch (msg)
            {
                case WM_DWMCOLORIZATIONCOLORCHANGED:
                case WM_DWMCOMPOSITIONCHANGED:
                case WM_THEMECHANGED:
                    ThemeChanged?.Invoke();
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private static Color GetWindowBackgroundColor()
        {
            string resource;
            if (SystemParameters.HighContrast)
            {
                resource = "ImmersiveApplicationBackground";
            }
            else if (UserSystemPreferencesService.UseAccentColor)
            {
                resource = IsWindowTransparencyEnabled ? "ImmersiveSystemAccentDark2" : "ImmersiveSystemAccentDark1";
            }
            else
            {
                resource = "ImmersiveDarkChromeMedium";
            }

            var color = AccentColorService.GetColorByTypeName(resource);
            color.A = (byte) (IsWindowTransparencyEnabled ? 190 : 255);
            return color;
        }

        private static SolidColorBrush Lookup(string name, double opacity = 1)
        {
            var color = AccentColorService.GetColorByTypeName(name);
            color.A = (byte)(opacity * 255);
            return new SolidColorBrush(color);
        }
    }
}