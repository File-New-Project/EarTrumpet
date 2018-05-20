using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarTrumpet.Services
{
    public class ThemeService : ViewModels.BindableBase
    {
        public event Action ThemeChanged;

        public bool AnimationsEnabled => SystemParameters.MenuAnimation;

        private bool IsWindowTransparencyEnabled
        {
            get { return !SystemParameters.HighContrast && UserSystemPreferencesService.IsTransparencyEnabled; }
        }

        public void LoadCurrentTheme()
        {
            var newDictionary = new ResourceDictionary();
            var themeDictionary = Application.Current.Resources.MergedDictionaries[0];
            var isLightTheme = UserSystemPreferencesService.IsLightTheme;

            newDictionary["WindowForeground"] = Lookup("ImmersiveApplicationTextDarkTheme");
            newDictionary["HeaderBackground"] = Lookup("ImmersiveSystemAccentLight1", 0.2);
            newDictionary["HeaderBackgroundSolid"] = Lookup("ImmersiveSystemAccent", 1);
            newDictionary["CottonSwabSliderThumb"] = Lookup("ImmersiveSystemAccent");
            newDictionary["ActiveBorder"] = Lookup("ImmersiveSystemAccent");
            newDictionary["CottonSwabSliderThumbHover"] = Lookup("ImmersiveControlDarkSliderThumbHover");
            newDictionary["CottonSwabSliderThumbPressed"] = Lookup("ImmersiveControlDarkSliderThumbHover");

            newDictionary["CottonSwabSliderTrackFill"] = Lookup("ImmersiveSystemAccentLight1");
            newDictionary["WindowBackground"] = new SolidColorBrush(GetWindowBackgroundColor());

            var blurColor = GetWindowBackgroundColor();
            var opacity = (UserSystemPreferencesService.IsTransparencyEnabled) ? 1 : 0.9;
            blurColor.A = (byte)(opacity * 255);

            newDictionary["BlurBackground"] = new SolidColorBrush(blurColor);
            newDictionary["PopupBackground"] = new SolidColorBrush(GetWindowBackgroundColor());
            newDictionary["PeakMeterHotColor"] = Lookup("ImmersiveSystemAccentDark3", 0.9);

            newDictionary["NormalWindowForeground"] = Lookup(isLightTheme ? "ImmersiveApplicationTextLightTheme" : "ImmersiveApplicationTextDarkTheme");
            newDictionary["NormalWindowBackground"] = Lookup("ImmersiveApplicationBackground");
            newDictionary["ButtonBackground"] = Lookup(isLightTheme ? "ImmersiveLightBaseLow" : "ImmersiveDarkBaseLow");
            newDictionary["ButtonBackgroundHover"] = Lookup(isLightTheme ? "ImmersiveLightBaseLow" : "ImmersiveDarkBaseLow");
            newDictionary["ButtonBackgroundPressed"] = Lookup(isLightTheme ? "ImmersiveLightBaseMediumLow" : "ImmersiveDarkBaseMediumLow");
            newDictionary["ButtonBorder"] = new SolidColorBrush(Colors.Transparent);
            newDictionary["ButtonBorderHover"] = Lookup(isLightTheme ? "ImmersiveLightBaseMediumLow" : "ImmersiveDarkBaseMediumLow");
            newDictionary["ButtonBorderPressed"] = new SolidColorBrush(Colors.Transparent);
            newDictionary["ButtonForeground"] = Lookup(isLightTheme ? "ImmersiveLightBaseHigh" : "ImmersiveDarkBaseHigh");
            newDictionary["ButtonForegroundHover"] = Lookup(isLightTheme ? "ImmersiveLightBaseHigh" : "ImmersiveDarkBaseHigh");
            newDictionary["ButtonForegroundPressed"] = Lookup(isLightTheme ? "ImmersiveLightBaseHigh" : "ImmersiveDarkBaseHigh");
            newDictionary["LogoImage"] = new BitmapImage(new Uri(isLightTheme ? "pack://application:,,,/EarTrumpet;component/Assets/Logo-Light.png" : "pack://application:,,,/EarTrumpet;component/Assets/Logo-Dark.png"));

            newDictionary["InactiveWindowBorder"] = newDictionary["NormalWindowBackground"];

            newDictionary["HyperlinkTextForeground"] = Lookup("ImmersiveSystemAccent");
            newDictionary["HyperlinkTextForegroundHover"] = Lookup(isLightTheme ? "ImmersiveLightBaseMedium" : "ImmersiveDarkBaseMedium");

            newDictionary["PeakMeterBackground"] = Lookup("ImmersiveSystemAccentDark3");

            if (SystemParameters.HighContrast)
            {
                newDictionary["FullWindowDeviceBackground"] = SystemColors.WindowBrush;
            }
            else
            {
                newDictionary["FullWindowDeviceBackground"] = Lookup(isLightTheme ? "ImmersiveLightListLow" : "ImmersiveDarkChromeMediumLow");
            }

            newDictionary["CloseButtonForeground"] = Lookup(isLightTheme ? "ImmersiveSystemText" : "ImmersiveApplicationTextDarkTheme");
            newDictionary["SettingsHeaderBackground"] = Lookup(isLightTheme ? "ImmersiveLightChromeMediumLow" : "ImmersiveDarkChromeMedium");


            newDictionary["SecondaryText"] = Lookup(isLightTheme ? "ImmersiveLightSecondaryText" : "ImmersiveLightDisabledText");
            newDictionary["SystemAccent"] = Lookup("ImmersiveSystemAccent");

            

            if (isLightTheme)
            {
                if (IsWindowTransparencyEnabled)
                {
                    newDictionary["ChromeBlackMedium"] = Lookup("ImmersiveLightChromeWhite", 0.7);
                }
                else
                {
                    newDictionary["ChromeBlackMedium"] = Lookup("ImmersiveLightAcrylicWindowBackdropFallback", 1);

                }
            }
            else
            {
                newDictionary["ChromeBlackMedium"] = Lookup("ImmersiveDarkAcrylicWindowBackdropFallback", IsWindowTransparencyEnabled ? 0.6 : 1);

            }

            newDictionary["AcrylicWindowBackdropFallback"] = Lookup(isLightTheme ? "ImmersiveLightAcrylicWindowBackdropFallback" : "ImmersiveDarkAcrylicWindowBackdropFallback", 1);

            var AddThemeSpecificBrush = new Action<string>((s) =>
            {
                newDictionary[$"Control{s}"] = Lookup(isLightTheme ? $"ImmersiveControlLight{s}" : $"ImmersiveControlDark{s}");
            });

            AddThemeSpecificBrush("SliderTrackFillRest");
            AddThemeSpecificBrush("SliderTrackFillDisabled");
            AddThemeSpecificBrush("SliderThumbHover");

            newDictionary["HardwareTitleBarCloseButtonHover"] = Lookup("ImmersiveHardwareTitleBarCloseButtonHover", 1);
            newDictionary["HardwareTitleBarCloseButtonPressed"] = Lookup("ImmersiveHardwareTitleBarCloseButtonPressed", 1);

            Application.Current.Resources.MergedDictionaries.Remove(themeDictionary);
            Application.Current.Resources.MergedDictionaries.Insert(0, newDictionary);
        }

        public void RegisterForThemeChanges(IntPtr hwnd)
        {
            var src = HwndSource.FromHwnd(hwnd);
            src.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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
                    else if (settingChanged == "WindowMetrics")
                    {
                        RaisePropertyChanged(nameof(AnimationsEnabled));
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private Color GetWindowBackgroundColor()
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
            color.A = (byte) (IsWindowTransparencyEnabled ? 180 : 255);
            return color;
        }

        private SolidColorBrush Lookup(string name, double opacity = 0)
        {
            var color = AccentColorService.GetColorByTypeName(name);
            if (opacity > 0)
            {
                color.A = (byte)(opacity * 255);
            }
            return new SolidColorBrush(color);
        }
    }
}