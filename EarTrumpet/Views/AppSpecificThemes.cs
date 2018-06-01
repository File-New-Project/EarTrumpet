using EarTrumpet.Services;
using System.Collections.Generic;
using System.Windows.Media;

namespace EarTrumpet.Views
{
    internal class AppSpecificThemes
    {
        public static Dictionary<string, ThemeService.IResolvableThemeBrush> GetThemeBuildData()
        {
            return new Dictionary<string, ThemeService.IResolvableThemeBrush>
            {
                { "WindowForeground", new Lookup("ImmersiveApplicationTextDarkTheme") },
                { "HeaderBackground", new Lookup("ImmersiveSystemAccentLight1", 0.2) },
                { "HeaderBackgroundSolid", new Lookup("ImmersiveSystemAccent", 1) },
                { "CottonSwabSliderThumb", new Lookup("ImmersiveSystemAccent") },
                { "ActiveBorder", new Lookup("ImmersiveSystemAccent") },
                { "CottonSwabSliderThumbHover", new Lookup("ImmersiveControlDarkSliderThumbHover") },
                { "CottonSwabSliderThumbPressed", new Lookup("ImmersiveControlDarkSliderThumbHover") },
                { "CottonSwabSliderTrackFill", new Lookup("ImmersiveSystemAccentLight1") },
                { "WindowBackground", new WindowBackground(0.7, opacityNotTransparent: 1) },
                { "PopupBackground", new WindowBackground(0.7, opacityNotTransparent: 1) },
                { "BlurBackground", new WindowBackground(1, opacityNotTransparent: 0.9) },
                { "PeakMeterHotColor", new Static(Colors.White) },
                { "NormalWindowForeground", new LightOrDark(new Lookup("ImmersiveApplicationTextLightTheme"), new Lookup("ImmersiveApplicationTextDarkTheme")) },
                { "NormalWindowBackground", new LightOrDark(new Lookup("ImmersiveApplicationBackground"), new Static(Color.FromArgb(255, 34, 34, 34))) },
                { "InactiveWindowBorder", new LightOrDark(new Lookup("ImmersiveApplicationBackground"), new Static(Color.FromArgb(255, 34, 34, 34))) },
                { "ButtonBackground", new LightOrDark(new Lookup("ImmersiveLightBaseLow"), new Lookup("ImmersiveDarkBaseLow")) },
                { "ButtonBackgroundHover", new LightOrDark(new Lookup("ImmersiveLightBaseLow"), new Lookup("ImmersiveDarkBaseLow")) },
                { "ButtonBackgroundPressed", new LightOrDark(new Lookup("ImmersiveLightBaseMediumLow"), new Lookup("ImmersiveDarkBaseMediumLow")) },
                { "ButtonBorder", new Static(Colors.Transparent) },
                { "ButtonBorderHover", new LightOrDark(new Lookup("ImmersiveLightBaseMediumLow"), new Lookup("ImmersiveDarkBaseMediumLow")) },
                { "ButtonBorderPressed", new Static(Colors.Transparent) },
                { "ButtonForeground", new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh")) },
                { "ButtonForegroundHover", new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh")) },
                { "ButtonForegroundPressed", new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh")) },
                { "HyperlinkTextForeground", new Lookup("ImmersiveSystemAccent") },
                { "HyperlinkTextForegroundHover", new LightOrDark(new Lookup("ImmersiveLightBaseMedium"), new Lookup("ImmersiveDarkBaseMedium")) },
                { "PeakMeterBackground", new LightOrDark(new Lookup("ImmersiveSystemAccentDark3"), new Static(Colors.White)) },
                { "CloseButtonForeground", new LightOrDark(new Lookup("ImmersiveSystemText"), new Lookup("ImmersiveApplicationTextDarkTheme")) },
                { "SettingsHeaderBackground", new LightOrDark(new Lookup("ImmersiveLightChromeMediumLow"), new Static(Color.FromArgb(255, 48, 48, 48))) },
                { "SecondaryText", new LightOrDark(new Lookup("ImmersiveLightSecondaryText"), new Lookup("ImmersiveLightDisabledText")) },
                { "SystemAccent", new Lookup("ImmersiveSystemAccent") },
                { "FullWindowDeviceBackground", new LightOrDark(new Lookup("ImmersiveLightListLow"), new Lookup("ImmersiveDarkChromeMediumLow"), new Static(System.Windows.SystemColors.WindowColor)) },
                { "AcrylicWindowBackdropFallback", new LightOrDark(new Lookup("ImmersiveLightAcrylicWindowBackdropFallback"), new Lookup("ImmersiveDarkAcrylicWindowBackdropFallback", 1)) },
                { "HardwareTitleBarCloseButtonHover", new Lookup("ImmersiveHardwareTitleBarCloseButtonHover", 1) },
                { "HardwareTitleBarCloseButtonPressed", new Lookup("ImmersiveHardwareTitleBarCloseButtonPressed", 1) },
                { "ControlSliderTrackFillRest", new LightOrDark(new Lookup("ImmersiveControlLightSliderTrackFillRest"), new Lookup("ImmersiveControlDarkSliderTrackFillRest")) },
                { "ControlSliderTrackFillDisabled", new LightOrDark(new Lookup("ImmersiveControlLightSliderTrackFillDisabled"), new Lookup("ImmersiveControlDarkSliderTrackFillDisabled")) },
                { "ControlSliderThumbHover", new LightOrDark(new Lookup("ImmersiveControlLightSliderThumbHover"), new Lookup("ImmersiveControlDarkSliderThumbHover")) },
                { "ChromeBlackMedium",
                    new LightOrDark(
                        new TransparentOrNot(
                            new Lookup("ImmersiveLightChromeWhite", 0.7),
                            new Lookup("ImmersiveLightAcrylicWindowBackdropFallback", 1)),
                        new Lookup("ImmersiveDarkAcrylicWindowBackdropFallback", 0.6, 1))
                },
                { "ContextMenuBackground", new LightOrDark(new Lookup("ImmersiveLightChromeMediumLow"), new Lookup("ImmersiveDarkChromeMediumLow")) },
                { "ContextMenuBorder", new LightOrDark(new Lookup("ImmersiveLightChromeHigh"), new Lookup("ImmersiveControlDarkAppButtonTextDisabled", 0.9)) },
                { "ContextMenuItemBackgroundHover", new LightOrDark(new Lookup("ImmersiveLightListLow"), new Lookup("ImmersiveDarkListLow")) },
                { "ContextMenuItemText", new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh")) },
                { "ContextMenuItemTextHover", new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh")) },
                { "ContextMenuItemTextDisabled", new LightOrDark(new Lookup("ImmersiveLightBaseMediumLow"), new Lookup("ImmersiveDarkBaseMediumLow")) },
                { "ContextMenuItemGlyph", new LightOrDark(new Lookup("ImmersiveLightBaseMedium"), new Lookup("ImmersiveDarkBaseMedium")) },
                { "ContextMenuSeparator", new LightOrDark(new Lookup("ImmersiveLightBaseMediumLow"), new Lookup("ImmersiveDarkBaseMediumLow")) },

                { "ContextMenuBackgroundDarkOnly", new Lookup("ImmersiveDarkChromeMediumLow") },
                { "ContextMenuBorderDarkOnly", new Lookup("ImmersiveControlDarkAppButtonTextDisabled", 0.9) },
                { "ContextMenuItemBackgroundHoverDarkOnly", new Lookup("ImmersiveDarkListLow") },
                { "ContextMenuItemTextDarkOnly", new Lookup("ImmersiveDarkBaseHigh") },
                { "ContextMenuItemTextHoverDarkOnly", new Lookup("ImmersiveDarkBaseHigh") },
                { "ContextMenuItemTextDisabledDarkOnly", new Lookup("ImmersiveDarkBaseMediumLow") },
                { "ContextMenuItemGlyphDarkOnly", new Lookup("ImmersiveDarkBaseMedium") },
                { "ContextMenuSeparatorDarkOnly",  new Lookup("ImmersiveDarkBaseMediumLow") },

                { "CheckBoxBorder",  new LightOrDark(new Lookup("ImmersiveLightBaseMediumHigh"), new Lookup("ImmersiveDarkBaseMediumHigh")) },
                { "CheckBoxBorderHover",  new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh")) },
                { "CheckBoxBorderPressed",  new LightOrDark(new Lookup("ImmersiveLightBaseMediumHigh"), new Lookup("ImmersiveDarkBaseMedium")) },
                { "CheckBoxBorderChecked",  new Lookup("ImmersiveSystemAccent") },
                { "CheckBoxBackground",  new Static(Colors.Transparent) },
                { "CheckBoxBackgroundHover",  new Static(Colors.Transparent) },
                { "CheckBoxBackgroundPressed",  new LightOrDark(new Lookup("ImmersiveLightBaseMediumHigh"), new Lookup("ImmersiveDarkBaseMedium")) },
                { "CheckBoxBackgroundChecked",  new Lookup("ImmersiveSystemAccent") }
            };
        }

        private class WindowBackground : ThemeService.IResolvableThemeBrush
        {
            private readonly double _opacityTransparent;
            private readonly double _opacityNotTransparent;

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
                    resource = "ImmersiveDarkChromeLow";
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

        private class Static : ThemeService.IResolvableThemeBrush
        {
            private readonly Color _color;

            public Static(Color color)
            {
                _color = color;
            }

            public Color Resolve(ThemeService.ThemeResolveData data)
            {
                return _color;
            }
        }

        private class Lookup : ThemeService.IResolvableThemeBrush
        {
            private readonly string _color;
            private readonly double _opacity;
            private readonly double _opacityWhenNotTransparent;

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

        private class TransparentOrNot : ThemeService.IResolvableThemeBrush
        {
            readonly ThemeService.IResolvableThemeBrush _transparentColor;
            readonly ThemeService.IResolvableThemeBrush _notTransparentColor;

            public TransparentOrNot(ThemeService.IResolvableThemeBrush transparentColor, ThemeService.IResolvableThemeBrush notTransparentColor)
            {
                _transparentColor = transparentColor;
                _notTransparentColor = notTransparentColor;
            }

            public Color Resolve(ThemeService.ThemeResolveData data)
            {
                return data.IsTransparencyEnabled ? _transparentColor.Resolve(data) : _notTransparentColor.Resolve(data);
            }
        }

        private class LightOrDark : ThemeService.IResolvableThemeBrush
        {
            readonly ThemeService.IResolvableThemeBrush _lightColor;
            readonly ThemeService.IResolvableThemeBrush _darkColor;
            readonly ThemeService.IResolvableThemeBrush _highContrastColor;

            public LightOrDark(ThemeService.IResolvableThemeBrush lightcolor, ThemeService.IResolvableThemeBrush darkColor, ThemeService.IResolvableThemeBrush highContrastColor = null)
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
