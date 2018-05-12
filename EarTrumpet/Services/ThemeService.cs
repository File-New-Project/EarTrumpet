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

        public static void UpdateThemeResources(ResourceDictionary dictionary)
        {
            dictionary["WindowBackground"] = new SolidColorBrush(GetWindowBackgroundColor());

            SetBrush(dictionary, "WindowForeground", "ImmersiveApplicationTextDarkTheme");
            ReplaceBrush(dictionary, "CottonSwabSliderThumb", "ImmersiveSystemAccent");
            ReplaceBrush(dictionary, "ActiveBorder", "ImmersiveSystemAccent");
            ReplaceBrushWithOpacity(dictionary, "HeaderBackground", "ImmersiveSystemAccent", 0.2);
            ReplaceBrushWithOpacity(dictionary, "HeaderBackgroundSolid", "ImmersiveSystemAccent", 0.6);
            ReplaceBrushWithOpacity(dictionary, "BadgeBackground", "ImmersiveSystemAccentDark2", 0.8);
            ReplaceBrush(dictionary, "CottonSwabSliderTrackFill", "ImmersiveSystemAccentLight1");
            SetBrush(dictionary, "CottonSwabSliderThumbHover", "ImmersiveControlDarkSliderThumbHover");
            SetBrush(dictionary, "CottonSwabSliderThumbPressed", "ImmersiveControlDarkSliderThumbHover");
            SetBrush(dictionary, "PeakMeterHotColor", IsWindowTransparencyEnabled ? "ImmersiveSystemAccentDark2" : "ImmersiveSystemAccentDark3");
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

        private static void SetBrushWithOpacity(ResourceDictionary dictionary, string name, string immersiveAccentName, double opacity)
        {
            if (!dictionary.Contains(name)) return;

            var color = AccentColorService.GetColorByTypeName(immersiveAccentName);
            color.A = (byte) (opacity*255);
            ((SolidColorBrush) dictionary[name]).Color = color;
        }

        private static void SetBrush(ResourceDictionary dictionary, string name, string immersiveAccentName)
        {
            SetBrushWithOpacity(dictionary, name, immersiveAccentName, 1.0);
        }

        private static void ReplaceBrush(ResourceDictionary dictionary, string name, string immersiveAccentName)
        {
            if (!dictionary.Contains(name)) return;

            dictionary[name] = new SolidColorBrush(AccentColorService.GetColorByTypeName(immersiveAccentName));
        }
        private static void ReplaceBrushWithOpacity(ResourceDictionary dictionary, string name, string immersiveAccentName, double opacity)
        {
            if (!dictionary.Contains(name)) return;

            var color = AccentColorService.GetColorByTypeName(immersiveAccentName);
            color.A = (byte)(opacity * 255);
            dictionary[name] = new SolidColorBrush(color);
        }
    }
}