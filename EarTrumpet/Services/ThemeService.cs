using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            var isLightTheme = UserSystemPreferencesService.IsLightTheme;

            newDictionary["WindowForeground"] = Lookup("ImmersiveApplicationTextDarkTheme");
            newDictionary["HeaderBackground"] = Lookup("ImmersiveSystemAccent", 0.2);
            newDictionary["HeaderBackgroundSolid"] = Lookup("ImmersiveSystemAccent", 0.6);
            newDictionary["CottonSwabSliderThumb"] = Lookup("ImmersiveSystemAccent");
            newDictionary["ActiveBorder"] = Lookup("ImmersiveSystemAccent");
            newDictionary["CottonSwabSliderThumbHover"] = Lookup("ImmersiveControlDarkSliderThumbHover");
            newDictionary["CottonSwabSliderThumbPressed"] = Lookup("ImmersiveControlDarkSliderThumbHover");
            newDictionary["CottonSwabSliderTrackFill"] = Lookup("ImmersiveSystemAccentLight1");
            newDictionary["BadgeBackground"] = Lookup("ImmersiveSystemAccentDark2", 0.8);
            newDictionary["WindowBackground"] = new SolidColorBrush(GetWindowBackgroundColor());
            newDictionary["PopupBackground"] = new SolidColorBrush(GetWindowBackgroundColor(true));
            newDictionary["PeakMeterHotColor"] = Lookup(IsWindowTransparencyEnabled ? "ImmersiveSystemAccentDark2" : "ImmersiveSystemAccentDark3");


            newDictionary["NormalWindowForeground"] = Lookup(isLightTheme ? "ImmersiveApplicationTextLightTheme" : "ImmersiveApplicationTextDarkTheme");
            newDictionary["NormalWindowBackground"] = Lookup("ImmersiveApplicationBackground");
            newDictionary["ButtonBackground"] = Lookup("ImmersiveLightBaseLow");
            newDictionary["ButtonBackgroundHover"] = Lookup("ImmersiveLightBaseLow");
            newDictionary["ButtonBackgroundPressed"] = Lookup("ImmersiveLightBaseMediumLow");
            newDictionary["ButtonBorder"] = new SolidColorBrush(new Color() { A = 0, R = 0, G = 0, B = 0 });
            newDictionary["ButtonBorderHover"] = Lookup("ImmersiveLightBaseMediumLow");
            newDictionary["ButtonBorderPressed"] = new SolidColorBrush(new Color() { A = 0, R = 0, G = 0, B = 0 });
            newDictionary["ButtonForeground"] = Lookup("ImmersiveLightBaseHigh");
            newDictionary["ButtonForegroundHover"] = Lookup("ImmersiveLightBaseHigh");
            newDictionary["ButtonForegroundPressed"] = Lookup("ImmersiveLightBaseHigh");
            newDictionary["LogoImage"] = new BitmapImage(new Uri(isLightTheme ? "pack://application:,,,/EarTrumpet;component/Assets/Logo-Light.png" : "pack://application:,,,/EarTrumpet;component/Assets/Logo-Dark.png"));

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
            const int WM_SETTINGCHANGE = 0x001A;

            switch (msg)
            {
                case WM_DWMCOLORIZATIONCOLORCHANGED:
                case WM_DWMCOMPOSITIONCHANGED:
                case WM_THEMECHANGED:                
                    ThemeChanged?.Invoke();
                    break;
                case WM_SETTINGCHANGE:
                    var settingChanged = Marshal.PtrToStringUni(lParam);
                    if (settingChanged == "ImmersiveColorSet")
                    {
                        ThemeChanged?.Invoke();
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private static Color GetWindowBackgroundColor(bool solidOverride = false)
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
            if (solidOverride)
            {
                color.A = 255;
            }
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