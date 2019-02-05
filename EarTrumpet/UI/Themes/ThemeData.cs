using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace EarTrumpet.UI.Themes
{
    public class ThemeData
    {
        public static Dictionary<string, IResolveColor> GetBrushData()
        {
            return new Dictionary<string, IResolveColor>
            {

                { "Unicolor_DevicePaneBackgroundLayer1", new Lookup("ImmersiveSystemAccent", 0.3, 0.5, SystemColors.WindowColor)},
                { "Unicolor_DevicePaneBackgroundLayer2", new NormalOrHC(new UnicolorWindowBackground(0.6, opacityNotTransparent: 1), new Static(Colors.Transparent)) },
                { "Unicolor_Background", new UnicolorWindowBackground(0.7, opacityNotTransparent: 1) },
                { "Unicolor_BlurBackground", new UnicolorWindowBackground(1, opacityNotTransparent: 0.9) },

                { "LightOrDark_Background", new LightOrDark(new Lookup("ImmersiveApplicationBackground"), new Static(Color.FromArgb(255, 34, 34, 34)), new Static(SystemColors.WindowColor)) },
                { "LightOrDark_SecondaryText", new LightOrDark(new Lookup("ImmersiveLightSecondaryText"), new Lookup("ImmersiveLightDisabledText")) },
                { "LightOrDark_VirtualTitleBarBackground", new LightOrDark(new Lookup("ImmersiveLightListLow"), new Lookup("ImmersiveDarkChromeMediumLow"), new Static(System.Windows.SystemColors.WindowColor)) },
                { "LightOrDark_VirtualTitleBarButtonHoverBackground", new LightOrDark(new Lookup("ImmersiveLightBaseMediumLow", 0.12), new Lookup("ImmersiveDarkBaseMediumLow", 0.12), new Static(SystemColors.HighlightColor)) },
                { "LightOrDark_VirtualTitleBarButtonHoverText", new LightOrDark(new Lookup("ImmersiveApplicationTextLightTheme"), new Lookup("ImmersiveApplicationTextDarkTheme"), new Static(SystemColors.HighlightTextColor)) },
                { "LightOrDark_VirtualTitleBarButtonPressedBackground", new LightOrDark(new Lookup("ImmersiveLightBaseMediumLow", 0.2), new Lookup("ImmersiveDarkBaseMediumLow", 0.2), new Static(SystemColors.HighlightColor)) },
                { "LightOrDark_VirtualTitleBarButtonPressedText", new LightOrDark(new Lookup("ImmersiveApplicationTextLightTheme"), new Lookup("ImmersiveApplicationTextDarkTheme"), new Static(SystemColors.HighlightTextColor)) },
                { "LightOrDark_AcrylicBackgroundFallback", new LightOrDark(new Lookup("ImmersiveLightAcrylicWindowBackdropFallback"), new Lookup("ImmersiveDarkAcrylicWindowBackdropFallback", 1)) },

                { "LightOrDark_ActiveAcrylicBackground", new LightOrDark(
                                            new TransparentOrNot(
                                                new Lookup("ImmersiveLightChromeWhite", 0.7),
                                                new Lookup("ImmersiveLightAcrylicWindowBackdropFallback", 1)),
                                            new Lookup("ImmersiveDarkAcrylicWindowBackdropFallback", 0.6, 1))},
                { "LightOrDark_ActiveWindowBorder", new Lookup("ImmersiveSystemAccent") },
                { "LightOrDark_InactiveWindowBorder", new LightOrDark(new Lookup("ImmersiveApplicationBackground"), new Static(Color.FromArgb(255, 34, 34, 34))) },
            };
        }

        private class UnicolorWindowBackground : IResolveColor
        {
            private readonly double _opacityTransparent;
            private readonly double _opacityNotTransparent;

            public UnicolorWindowBackground(double opacityTransparent, double opacityNotTransparent)
            {
                _opacityTransparent = opacityTransparent;
                _opacityNotTransparent = opacityNotTransparent;
            }

            public Color Resolve(IResolveColorOptions data)
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

        private class Static : IResolveColor
        {
            private readonly Color _color;
            private readonly Color _highContrastColor;

            public Static(Color color, Color highContrastColor = default(Color))
            {
                _color = color;
                _highContrastColor = highContrastColor;
            }

            public Color Resolve(IResolveColorOptions data)
            {
                if (data.IsHighContrast && _highContrastColor != default(Color))
                {
                    return _highContrastColor;
                }
                return _color;
            }
        }

        private class Lookup : IResolveColor
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

            public Color Resolve(IResolveColorOptions data)
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

        private class TransparentOrNot : IResolveColor
        {
            readonly IResolveColor _transparentColor;
            readonly IResolveColor _notTransparentColor;

            public TransparentOrNot(IResolveColor transparentColor, IResolveColor notTransparentColor)
            {
                _transparentColor = transparentColor;
                _notTransparentColor = notTransparentColor;
            }

            public Color Resolve(IResolveColorOptions data)
            {
                return data.IsTransparencyEnabled ? _transparentColor.Resolve(data) : _notTransparentColor.Resolve(data);
            }
        }

        private class LightOrDark : IResolveColor
        {
            readonly IResolveColor _lightColor;
            readonly IResolveColor _darkColor;
            readonly IResolveColor _highContrastColor;

            public LightOrDark(IResolveColor lightcolor, IResolveColor darkColor, IResolveColor highContrastColor = null)
            {
                _lightColor = lightcolor;
                _darkColor = darkColor;
                _highContrastColor = highContrastColor;
            }

            public Color Resolve(IResolveColorOptions data)
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

        private class NormalOrHC : IResolveColor
        {
            readonly IResolveColor _normal;
            readonly IResolveColor _highContrast;

            public NormalOrHC(IResolveColor normal, IResolveColor highContrast)
            {
                _normal = normal;
                _highContrast = highContrast;
            }

            public Color Resolve(IResolveColorOptions data)
            {
                return data.IsHighContrast ? _highContrast.Resolve(data) : _normal.Resolve(data);
            }
        }
    }
}