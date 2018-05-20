using EarTrumpet.Services;
using System.Collections.Generic;
using System.Windows.Media;

namespace EarTrumpet.Views
{
    class AppSpecificThemes
    {
        public static Dictionary<string, ThemeService.ResolvableThemeBrush> GetThemeBuildData()
        {
            var ret = new Dictionary<string, ThemeService.ResolvableThemeBrush>();

            ret.Add("WindowForeground", new Lookup("ImmersiveApplicationTextDarkTheme"));
            ret.Add("HeaderBackground", new Lookup("ImmersiveSystemAccentLight1", 0.2));
            ret.Add("HeaderBackgroundSolid", new Lookup("ImmersiveSystemAccent", 1));
            ret.Add("CottonSwabSliderThumb", new Lookup("ImmersiveSystemAccent"));
            ret.Add("ActiveBorder", new Lookup("ImmersiveSystemAccent"));
            ret.Add("CottonSwabSliderThumbHover", new Lookup("ImmersiveControlDarkSliderThumbHover"));
            ret.Add("CottonSwabSliderThumbPressed", new Lookup("ImmersiveControlDarkSliderThumbHover"));
            ret.Add("CottonSwabSliderTrackFill", new Lookup("ImmersiveSystemAccentLight1"));
            ret.Add("WindowBackground", new WindowBackground(0.7, opacityNotTransparent: 1));
            ret.Add("PopupBackground", new WindowBackground(0.7, opacityNotTransparent: 1));
            ret.Add("BlurBackground", new WindowBackground(1, opacityNotTransparent: 0.9));
            ret.Add("PeakMeterHotColor", new Lookup("ImmersiveSystemAccentDark3", 0.9));
            ret.Add("NormalWindowForeground", new LightOrDark(new Lookup("ImmersiveApplicationTextLightTheme"), new Lookup("ImmersiveApplicationTextDarkTheme")));

            var normalWindowBackground = new LightOrDark(new Lookup("ImmersiveApplicationBackground"), new Static(Color.FromArgb(255, 34, 34, 34)));
            ret.Add("NormalWindowBackground", normalWindowBackground);
            ret.Add("InactiveWindowBorder", normalWindowBackground);
            ret.Add("ButtonBackground", new LightOrDark(new Lookup("ImmersiveLightBaseLow"), new Lookup("ImmersiveDarkBaseLow")));
            ret.Add("ButtonBackgroundHover", new LightOrDark(new Lookup("ImmersiveLightBaseLow"), new Lookup("ImmersiveDarkBaseLow")));
            ret.Add("ButtonBackgroundPressed", new LightOrDark(new Lookup("ImmersiveLightBaseMediumLow"), new Lookup("ImmersiveDarkBaseMediumLow")));
            ret.Add("ButtonBorder", new Static(Colors.Transparent));
            ret.Add("ButtonBorderHover", new LightOrDark(new Lookup("ImmersiveLightBaseMediumLow"), new Lookup("ImmersiveDarkBaseMediumLow")));
            ret.Add("ButtonBorderPressed", new Static(Colors.Transparent));
            ret.Add("ButtonForeground", new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh")));
            ret.Add("ButtonForegroundHover", new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh")));
            ret.Add("ButtonForegroundPressed", new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh")));

            ret.Add("HyperlinkTextForeground", new Lookup("ImmersiveSystemAccent"));
            ret.Add("HyperlinkTextForegroundHover", new LightOrDark(new Lookup("ImmersiveLightBaseMedium"), new Lookup("ImmersiveDarkBaseMedium")));
            ret.Add("PeakMeterBackground", new Lookup("ImmersiveSystemAccentDark3"));
            ret.Add("CloseButtonForeground", new LightOrDark(new Lookup("ImmersiveSystemText"), new Lookup("ImmersiveApplicationTextDarkTheme")));
            ret.Add("SettingsHeaderBackground", new LightOrDark(new Lookup("ImmersiveLightChromeMediumLow"), new Static(Color.FromArgb(255, 48, 48, 48))));
            ret.Add("SecondaryText", new LightOrDark(new Lookup("ImmersiveLightSecondaryText"), new Lookup("ImmersiveLightDisabledText")));
            ret.Add("SystemAccent", new Lookup("ImmersiveSystemAccent"));
            ret.Add("FullWindowDeviceBackground", new LightOrDark(new Lookup("ImmersiveLightListLow"), new Lookup("ImmersiveDarkChromeMediumLow"), new Static(System.Windows.SystemColors.WindowColor)));
            ret.Add("AcrylicWindowBackdropFallback", new LightOrDark(new Lookup("ImmersiveLightAcrylicWindowBackdropFallback"), new Lookup("ImmersiveDarkAcrylicWindowBackdropFallback", 1)));
            ret.Add("HardwareTitleBarCloseButtonHover", new Lookup("ImmersiveHardwareTitleBarCloseButtonHover", 1));
            ret.Add("HardwareTitleBarCloseButtonPressed", new Lookup("ImmersiveHardwareTitleBarCloseButtonPressed", 1));
            ret.Add("ControlSliderTrackFillRest", new LightOrDark(new Lookup("ImmersiveControlLightSliderTrackFillRest"), new Lookup("ImmersiveControlDarkSliderTrackFillRest")));
            ret.Add("ControlSliderTrackFillDisabled", new LightOrDark(new Lookup("ImmersiveControlLightSliderTrackFillDisabled"), new Lookup("ImmersiveControlDarkSliderTrackFillDisabled")));
            ret.Add("ControlSliderThumbHover", new LightOrDark(new Lookup("ImmersiveControlLightSliderThumbHover"), new Lookup("ImmersiveControlDarkSliderThumbHover")));

            ret.Add("ChromeBlackMedium", new LightOrDark(
                                            new TransparentOrNot(
                                                new Lookup("ImmersiveLightChromeWhite", 0.7),
                                                new Lookup("ImmersiveLightAcrylicWindowBackdropFallback", 1)),
                                            new Lookup("ImmersiveDarkAcrylicWindowBackdropFallback", 0.6, 1)));

            return ret;
        }

        class WindowBackground : ThemeService.ResolvableThemeBrush
        {
            double _opacityTransparent;
            double _opacityNotTransparent;

            public WindowBackground(double opacityTransparent, double opacityNotTransparent)
            {
                _opacityTransparent = opacityTransparent;
                _opacityNotTransparent = opacityNotTransparent;
            }

            public Color Resolve(ThemeService.ThemeResolveData data)
            {
                string resource;
                if (data.IsHighContrast)
                {
                    resource = "ImmersiveApplicationBackground";
                }
                else if (data.UseAccentColor)
                {
                    resource = data.IsTransparencyEnabled ? "ImmersiveSystemAccentDark2" : "ImmersiveSystemAccentDark1";
                }
                else
                {
                    resource = "ImmersiveDarkChromeMedium";
                }

                var color = data.LookupThemeColor(resource);

                var opacity = data.IsTransparencyEnabled ? _opacityTransparent : _opacityNotTransparent;
                if (opacity > 0)
                {
                    color.A = (byte)(opacity * 255);
                }
                return color;
            }
        }

        class Static : ThemeService.ResolvableThemeBrush
        {
            readonly Color _color;

            public Static(Color color)
            {
                _color = color;
            }

            public Color Resolve(ThemeService.ThemeResolveData data)
            {
                return _color;
            }
        }

        class Lookup : ThemeService.ResolvableThemeBrush
        {
            readonly string _color;
            readonly double _opacity;
            readonly double _opacityWhenNotTransparent;

            public Lookup(string color, double opacity1 = 0, double opacityWhenNotTransparent = 0)
            {
                _color = color;
                _opacity = opacity1;
                _opacityWhenNotTransparent = opacityWhenNotTransparent;
            }

            public Color Resolve(ThemeService.ThemeResolveData data)
            {
                var color = data.LookupThemeColor(_color);

                var opacity = data.IsTransparencyEnabled ? _opacity :
                    (_opacityWhenNotTransparent > 0 ? _opacityWhenNotTransparent : _opacity);

                if (opacity > 0)
                {
                    color.A = (byte)(opacity * 255);
                }
                return color;
            }
        }

        class TransparentOrNot : ThemeService.ResolvableThemeBrush
        {
            readonly ThemeService.ResolvableThemeBrush _transparentColor;
            readonly ThemeService.ResolvableThemeBrush _notTransparentColor;

            public TransparentOrNot(ThemeService.ResolvableThemeBrush transparentColor, ThemeService.ResolvableThemeBrush notTransparentColor)
            {
                _transparentColor = transparentColor;
                _notTransparentColor = notTransparentColor;
            }

            public Color Resolve(ThemeService.ThemeResolveData data)
            {
                return data.IsTransparencyEnabled ? _transparentColor.Resolve(data) : _notTransparentColor.Resolve(data);
            }
        }

        class LightOrDark : ThemeService.ResolvableThemeBrush
        {
            readonly ThemeService.ResolvableThemeBrush _lightColor;
            readonly ThemeService.ResolvableThemeBrush _darkColor;
            readonly ThemeService.ResolvableThemeBrush _highContrastColor;

            public LightOrDark(ThemeService.ResolvableThemeBrush lightcolor, ThemeService.ResolvableThemeBrush darkColor, ThemeService.ResolvableThemeBrush highContrastColor = null)
            {
                _lightColor = lightcolor;
                _darkColor = darkColor;
                _highContrastColor = highContrastColor;
            }

            public Color Resolve(ThemeService.ThemeResolveData data)
            {
                if (_highContrastColor != null && data.IsHighContrast) return _highContrastColor.Resolve(data);

                return data.IsLightTheme ? _lightColor.Resolve(data) : _darkColor.Resolve(data);
            }
        }
    }
}
