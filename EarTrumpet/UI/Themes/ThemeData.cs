using EarTrumpet.UI.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace EarTrumpet.UI.Themes
{
    internal class ThemeData
    {
        public static Dictionary<string, ThemeManager.IResolvableThemeBrush> GetBrushData()
        {
            return new Dictionary<string, ThemeManager.IResolvableThemeBrush>
            {
                // Flyout window "Unicolor" theme.
                { "Unicolor_Text", new Lookup("ImmersiveApplicationTextDarkTheme") },
                { "Unicolor_PrimaryFocusVisual", new Lookup("ImmersiveApplicationTextDarkTheme") },
                { "Unicolor_SecondaryFocusVisual", new Lookup("ImmersiveApplicationBackgroundDarkTheme") },
                { "Unicolor_ExpandCollapseButtonHoverBackground", new Static(Color.FromArgb(0x20, 0xff, 0xff, 0xff)) },
                { "Unicolor_ExpandCollapseButtonHoverText", new Lookup("ImmersiveApplicationTextDarkTheme") },
                { "Unicolor_ExpandCollapseButtonPressedBackground", new Static(Color.FromArgb(0x2f, 0xff, 0xff, 0xff)) },
                { "Unicolor_ExpandCollapseButtonPressedText", new Lookup("ImmersiveApplicationTextDarkTheme") },
                { "Unicolor_DevicePaneBackgroundLayer1", new Lookup("ImmersiveSystemAccent", 0.3, 0.5, SystemColors.WindowColor)},
                { "Unicolor_DevicePaneBackgroundLayer2", new NormalOrHC(new UnicolorWindowBackground(0.6, opacityNotTransparent: 1), new Static(Colors.Transparent)) },
                { "Unicolor_SliderThumb", new Lookup("ImmersiveSystemAccent", SystemColors.ControlLightColor) },
                { "Unicolor_SliderThumbHover", new Lookup("ImmersiveControlDarkSliderThumbHover", SystemColors.HighlightColor) },
                { "Unicolor_SliderThumbPressed", new Lookup("ImmersiveControlDarkSliderThumbHover", SystemColors.HighlightColor) },
                { "Unicolor_SliderTrackLeft", new Lookup("ImmersiveSystemAccentLight1") },
                { "Unicolor_SliderTrackRight", new Static(Color.FromArgb(0x39, 0xFF, 0xFF, 0xFF), SystemColors.ControlLightColor) },
                { "Unicolor_Background", new UnicolorWindowBackground(0.7, opacityNotTransparent: 1) },
                { "Unicolor_BlurBackground", new UnicolorWindowBackground(1, opacityNotTransparent: 0.9) },
                { "Unicolor_PeakMeterColor", new Static(Colors.White) },
                { "Unicolor_AppIconPlateBackground", new Lookup("ImmersiveSystemAccent") },
                { "Unicolor_VirtualTitleBarButtonHover", new Static(Color.FromArgb(0x22,0xff,0xff,0xff)) },
                { "LightOrDark_PeakMeterColor", new LightOrDark(new Lookup("ImmersiveApplicationTextLightTheme"), new Lookup("ImmersiveApplicationTextDarkTheme")) },
                { "LightOrDark_Text", new LightOrDark(new Lookup("ImmersiveApplicationTextLightTheme"), new Lookup("ImmersiveApplicationTextDarkTheme")) },
                { "LightOrDark_Background", new LightOrDark(new Lookup("ImmersiveApplicationBackground"), new Static(Color.FromArgb(255, 34, 34, 34)), new Static(SystemColors.WindowColor)) },
                { "LightOrDark_SettingsHeaderBackground", new LightOrDark(new Lookup("ImmersiveLightChromeMediumLow"), new Static(Color.FromArgb(255, 48, 48, 48)), new Static(SystemColors.WindowColor)) },
                { "LightOrDark_SecondaryText", new LightOrDark(new Lookup("ImmersiveLightSecondaryText"), new Lookup("ImmersiveLightDisabledText")) },
                { "LightOrDark_VirtualTitleBarBackground", new LightOrDark(new Lookup("ImmersiveLightListLow"), new Lookup("ImmersiveDarkChromeMediumLow"), new Static(System.Windows.SystemColors.WindowColor)) },
                { "LightOrDark_VirtualTitleBarButtonHover", new LightOrDark(new Lookup("ImmersiveLightBaseMediumLow", 0.2), new Lookup("ImmersiveDarkBaseMediumLow", 0.2)) },
                { "LightOrDark_AcrylicBackgroundFallback", new LightOrDark(new Lookup("ImmersiveLightAcrylicWindowBackdropFallback"), new Lookup("ImmersiveDarkAcrylicWindowBackdropFallback", 1)) },
                { "LightOrDark_SliderTrack", new LightOrDark(new Lookup("ImmersiveControlLightSliderTrackFillRest"), new Lookup("ImmersiveControlDarkSliderTrackFillRest")) },
                { "LightOrDark_SliderThumbHover", new LightOrDark(new Lookup("ImmersiveControlLightSliderThumbHover"), new Lookup("ImmersiveControlDarkSliderThumbHover")) },
                { "LightOrDark_SliderTrackRight", new LightOrDark(new Lookup("ImmersiveControlLightSliderTrackFillDisabled"), new Lookup("ImmersiveControlDarkSliderTrackFillDisabled"), new Static(SystemColors.ControlLightColor)) },
                { "LightOrDark_ActiveAcrylicBackground", new LightOrDark(
                                            new TransparentOrNot(
                                                new Lookup("ImmersiveLightChromeWhite", 0.7),
                                                new Lookup("ImmersiveLightAcrylicWindowBackdropFallback", 1)),
                                            new Lookup("ImmersiveDarkAcrylicWindowBackdropFallback", 0.6, 1))},
                { "LightOrDark_ActiveWindowBorder", new Lookup("ImmersiveSystemAccent") },
                { "LightOrDark_InactiveWindowBorder", new LightOrDark(new Lookup("ImmersiveApplicationBackground"), new Static(Color.FromArgb(255, 34, 34, 34))) },
                { "LightOrDark_CloseButtonText", new LightOrDark(new Lookup("ImmersiveSystemText"), new Lookup("ImmersiveApplicationTextDarkTheme")) },
                // Button is Light/Dark.
                { "ButtonBackground", new LightOrDark(new Lookup("ImmersiveLightBaseLow"), new Lookup("ImmersiveDarkBaseLow")) },
                { "ButtonBackgroundHover", new LightOrDark(new Lookup("ImmersiveLightBaseLow"), new Lookup("ImmersiveDarkBaseLow")) },
                { "ButtonBackgroundPressed", new LightOrDark(new Lookup("ImmersiveLightBaseMediumLow"), new Lookup("ImmersiveDarkBaseMediumLow")) },
                { "ButtonBorder", new NormalOrHC(new Static(Colors.Transparent), new Static(SystemColors.ControlTextColor)) },
                { "ButtonBorderHover", new LightOrDark(new Lookup("ImmersiveLightBaseMediumLow"), new Lookup("ImmersiveDarkBaseMediumLow"), new Static(SystemColors.ControlTextColor)) },
                { "ButtonBorderPressed", new Static(Colors.Transparent) },
                { "ButtonForeground", new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh")) },
                { "ButtonForegroundHover", new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh")) },
                { "ButtonForegroundPressed", new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh")) },
                // Hyperlink is Light/Dark.
                { "HyperlinkTextForeground", new NormalOrHC(new Lookup("ImmersiveSystemAccent"), new Lookup("ImmersiveControlDarkLinkRest")) },
                { "HyperlinkTextForegroundHover", new LightOrDark(new Lookup("ImmersiveLightBaseMedium"), new Lookup("ImmersiveDarkBaseMedium"), new Lookup("ImmersiveControlDarkLinkHover")) },
                // ContextMenu is Light/Dark.
                { "ContextMenuBackground", new LightOrDark(new Lookup("ImmersiveLightChromeMediumLow"), new Lookup("ImmersiveDarkChromeMediumLow"), new Static(SystemColors.MenuColor)) },
                { "ContextMenuBorder", new LightOrDark(new Lookup("ImmersiveLightChromeHigh"), new Lookup("ImmersiveControlDarkAppButtonTextDisabled", 0.9), new Static(SystemColors.ControlTextColor)) },
                { "ContextMenuItemBackgroundHover", new LightOrDark(new Lookup("ImmersiveLightListLow"), new Lookup("ImmersiveDarkListLow"), new Static(SystemColors.HighlightColor)) },
                { "ContextMenuItemText", new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh"), new Static(SystemColors.MenuTextColor)) },
                { "ContextMenuItemTextHover", new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh"), new Static(SystemColors.HighlightTextColor)) },
                { "ContextMenuItemTextDisabled", new LightOrDark(new Lookup("ImmersiveLightBaseMediumLow"), new Lookup("ImmersiveDarkBaseMediumLow")) },
                { "ContextMenuItemGlyph", new LightOrDark(new Lookup("ImmersiveLightBaseMedium"), new Lookup("ImmersiveDarkBaseMedium"), new Static(SystemColors.MenuTextColor)) },
                { "ContextMenuItemGlyphHover", new LightOrDark(new Lookup("ImmersiveLightBaseMedium"), new Lookup("ImmersiveDarkBaseMedium"), new Static(SystemColors.HighlightTextColor)) },
                { "ContextMenuSeparator", new LightOrDark(new Lookup("ImmersiveLightBaseMediumLow"), new Lookup("ImmersiveDarkBaseMediumLow"), new Static(SystemColors.MenuTextColor)) },
                { "ContextMenuBackgroundDarkOnly", new Lookup("ImmersiveDarkChromeMediumLow", SystemColors.MenuColor) },
                { "ContextMenuBorderDarkOnly", new Lookup("ImmersiveControlDarkAppButtonTextDisabled", 0.9, 0, SystemColors.ControlTextColor) },
                { "ContextMenuItemBackgroundHoverDarkOnly", new Lookup("ImmersiveDarkListLow", SystemColors.HighlightColor) },
                { "ContextMenuItemTextDarkOnly", new Lookup("ImmersiveDarkBaseHigh", SystemColors.MenuTextColor) },
                { "ContextMenuItemTextHoverDarkOnly", new Lookup("ImmersiveDarkBaseHigh", SystemColors.HighlightTextColor) },
                { "ContextMenuItemTextDisabledDarkOnly", new Lookup("ImmersiveDarkBaseMediumLow") },
                { "ContextMenuItemGlyphDarkOnly", new Lookup("ImmersiveDarkBaseMedium", SystemColors.MenuTextColor) },
                { "ContextMenuItemGlyphHoverDarkOnly", new Lookup("ImmersiveDarkBaseMedium", SystemColors.HighlightTextColor) },
                { "ContextMenuSeparatorDarkOnly",  new Lookup("ImmersiveDarkBaseMediumLow", SystemColors.MenuTextColor) },
                // CheckBox is Light/Dark.
                { "CheckBoxBorder",  new LightOrDark(new Lookup("ImmersiveLightBaseMediumHigh"), new Lookup("ImmersiveDarkBaseMediumHigh"), new Static(SystemColors.HighlightColor)) },
                { "CheckBoxBorderHover",  new LightOrDark(new Lookup("ImmersiveLightBaseHigh"), new Lookup("ImmersiveDarkBaseHigh"), new Static(SystemColors.HighlightColor)) },
                { "CheckBoxBorderPressed",  new LightOrDark(new Lookup("ImmersiveLightBaseMediumHigh"), new Lookup("ImmersiveDarkBaseMedium"), new Static(SystemColors.HighlightColor)) },
                { "CheckBoxBorderChecked",  new Lookup("ImmersiveSystemAccent") },
                { "CheckBoxBackground",  new Static(Colors.Transparent) },
                { "CheckBoxBackgroundHover",  new Static(Colors.Transparent) },
                { "CheckBoxBackgroundPressed",  new LightOrDark(new Lookup("ImmersiveLightBaseMediumHigh"), new Lookup("ImmersiveDarkBaseMedium")) },
                { "CheckBoxBackgroundChecked",  new Lookup("ImmersiveSystemAccent") }
            };
        }

        private class UnicolorWindowBackground : ThemeManager.IResolvableThemeBrush
        {
            private readonly double _opacityTransparent;
            private readonly double _opacityNotTransparent;

            public UnicolorWindowBackground(double opacityTransparent, double opacityNotTransparent)
            {
                _opacityTransparent = opacityTransparent;
                _opacityNotTransparent = opacityNotTransparent;
            }

            public Color Resolve(ThemeManager.ThemeResolveData data)
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

        private class Static : ThemeManager.IResolvableThemeBrush
        {
            private readonly Color _color;
            private readonly Color _highContrastColor;

            public Static(Color color, Color highContrastColor = default(Color))
            {
                _color = color;
                _highContrastColor = highContrastColor;
            }

            public Color Resolve(ThemeManager.ThemeResolveData data)
            {
                if (data.IsHighContrast && _highContrastColor != default(Color))
                {
                    return _highContrastColor;
                }
                return _color;
            }
        }

        private class Lookup : ThemeManager.IResolvableThemeBrush
        {
            private readonly string _color;
            private readonly double _opacity;
            private readonly double _opacityWhenNotTransparent;
            private readonly Color _highContrastColor;

            public Lookup(string color, double opacity1 = 0, double opacityWhenNotTransparent = 0)
            {
                _color = color;
                _opacity = opacity1;
                _opacityWhenNotTransparent = opacityWhenNotTransparent;
            }

            public Lookup(string color, double opacity1, double opacityWhenNotTransparent, Color highContrastColor)
            {
                _color = color;
                _opacity = opacity1;
                _opacityWhenNotTransparent = opacityWhenNotTransparent;
                _highContrastColor = highContrastColor;
            }

            public Lookup(string color, Color highContrastColor)
            {
                _color = color;
                _highContrastColor = highContrastColor;
            }

            public Color Resolve(ThemeManager.ThemeResolveData data)
            {
                if (_highContrastColor != default(Color) && data.IsHighContrast)
                {
                    return _highContrastColor;
                }

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

        private class TransparentOrNot : ThemeManager.IResolvableThemeBrush
        {
            readonly ThemeManager.IResolvableThemeBrush _transparentColor;
            readonly ThemeManager.IResolvableThemeBrush _notTransparentColor;

            public TransparentOrNot(ThemeManager.IResolvableThemeBrush transparentColor, ThemeManager.IResolvableThemeBrush notTransparentColor)
            {
                _transparentColor = transparentColor;
                _notTransparentColor = notTransparentColor;
            }

            public Color Resolve(ThemeManager.ThemeResolveData data)
            {
                return data.IsTransparencyEnabled ? _transparentColor.Resolve(data) : _notTransparentColor.Resolve(data);
            }
        }

        private class LightOrDark : ThemeManager.IResolvableThemeBrush
        {
            readonly ThemeManager.IResolvableThemeBrush _lightColor;
            readonly ThemeManager.IResolvableThemeBrush _darkColor;
            readonly ThemeManager.IResolvableThemeBrush _highContrastColor;

            public LightOrDark(ThemeManager.IResolvableThemeBrush lightcolor, ThemeManager.IResolvableThemeBrush darkColor, ThemeManager.IResolvableThemeBrush highContrastColor = null)
            {
                _lightColor = lightcolor;
                _darkColor = darkColor;
                _highContrastColor = highContrastColor;
            }

            public Color Resolve(ThemeManager.ThemeResolveData data)
            {
                if (_highContrastColor != null && data.IsHighContrast)
                {
                    return _highContrastColor.Resolve(data);
                }

                if (data.IsHighContrast)
                {
                    return _darkColor.Resolve(data);
                }

                return data.IsLightTheme ? _lightColor.Resolve(data) : _darkColor.Resolve(data);
            }
        }

        private class NormalOrHC : ThemeManager.IResolvableThemeBrush
        {
            readonly ThemeManager.IResolvableThemeBrush _normal;
            readonly ThemeManager.IResolvableThemeBrush _highContrast;

            public NormalOrHC(ThemeManager.IResolvableThemeBrush normal, ThemeManager.IResolvableThemeBrush highContrast)
            {
                _normal = normal;
                _highContrast = highContrast;
            }

            public Color Resolve(ThemeManager.ThemeResolveData data)
            {
                return data.IsHighContrast ? _highContrast.Resolve(data) : _normal.Resolve(data);
            }
        }
    }
}