using System.Windows;
using System.Windows.Media;

namespace EarTrumpet.Services
{
    public class ThemeService
    {
        public static bool IsWindowTransparencyEnabled
        {
            get { return !SystemParameters.HighContrast && UserSystemPreferencesService.IsTransparencyEnabled; }
        }

        public static void UpdateThemeResources(ResourceDictionary dictionary)
        {
            dictionary["WindowBackground"] = new SolidColorBrush(GetWindowBackgroundColor());

            SetBrush(dictionary, "WindowForeground", "ImmersiveApplicationTextDarkTheme");
            ReplaceBrush(dictionary, "CottonSwabSliderThumb", "ImmersiveSystemAccent");
            ReplaceBrushWithOpacity(dictionary, "HeaderBackground", "ImmersiveSystemAccent", 0.4);
            ReplaceBrush(dictionary, "CottonSwabSliderTrackFill", "ImmersiveSystemAccentLight1");
            SetBrush(dictionary, "CottonSwabSliderThumbHover", "ImmersiveControlDarkSliderThumbHover");
            SetBrush(dictionary, "CottonSwabSliderThumbPressed", "ImmersiveControlDarkSliderThumbHover");
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
            dictionary[name] = new SolidColorBrush(AccentColorService.GetColorByTypeName(immersiveAccentName));
        }
        private static void ReplaceBrushWithOpacity(ResourceDictionary dictionary, string name, string immersiveAccentName, double opacity)
        {
            var color = AccentColorService.GetColorByTypeName(immersiveAccentName);
            color.A = (byte)(opacity * 255);
            dictionary[name] = new SolidColorBrush(color);
        }
    }
}