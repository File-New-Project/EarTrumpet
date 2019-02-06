using EarTrumpet.DataModel;
using EarTrumpet.Interop.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace EarTrumpet.UI.Themes
{
    class BrushValueParser
    {
        public static object Parse(DependencyObject element, string value)
        {
            bool isLight = Options.GetSource(element) == Options.SourceKind.App ? SystemSettings.IsLightTheme : SystemSettings.IsSystemLightTheme;

            Dictionary<string, Dictionary<string, string>> fullMap = new Dictionary<string, Dictionary<string, string>>();
            // Transparent
            // Theme=Transparent
            // Flyout:Theme=Transparent, HighContrast=Window
            foreach (var commaValue in value.Split(','))
            {
                // Transparent
                // Theme=Transparent
                // Flyout:Theme=Transparent
                var segment = commaValue.Trim();
                var scope = "";
                if (segment.IndexOf(':') > -1)
                {
                    var colonSplit = segment.Split(':');
                    scope = colonSplit[0];
                    segment = colonSplit[1];
                }
                if (!fullMap.ContainsKey(scope))
                {
                    fullMap[scope] = new Dictionary<string, string>();
                }

                // Transparent
                // Theme=Transparent
                var equalsSplit = segment.Split('=');
                var key = equalsSplit.Length == 1 ? "" : equalsSplit[0];
                var color = equalsSplit.Length == 1 ? equalsSplit[0] : equalsSplit[1];
                fullMap[scope][key] = color;
            }

            // Search for Refs.
            var elementScope = Options.GetScope(element);
            var searchKey = "";
            if (fullMap.ContainsKey(elementScope))
            {
                if (fullMap[elementScope].ContainsKey(""))
                {
                    searchKey = fullMap[elementScope][""];
                }
            }
            else
            {
                if (fullMap[""].ContainsKey(""))
                {
                    searchKey = fullMap[""][""];
                }
            }
            if (searchKey != "")
            {
                searchKey = searchKey.Split('/')[0];
                var reference = Manager.Current.References.FirstOrDefault(r => r.Key == searchKey);
                if (reference != null)
                {
                    if (reference is AcrylicBackgroundRef)
                    {
                        string color;
                        if (isLight)
                        {
                            color = SystemSettings.IsTransparencyEnabled ? "LightChromeWhite/0.7" : "LightAcrylicWindowBackdropFallback/1";
                        }
                        else
                        {
                            color = "DarkAcrylicWindowBackdropFallback/0.6/1";
                        }
                        return Parse(element, $"Theme={color}");
                    }
                    else if (reference is FlyoutBackgroundRef)
                    {
                        string color;
                        if (SystemParameters.HighContrast)
                        {
                            return Parse(element, "Theme=ApplicationBackground");
                        }
                        else if (SystemSettings.UseAccentColor)
                        {
                            color = SystemSettings.IsTransparencyEnabled ? "SystemAccentDark2" : "SystemAccentDark1";
                        }
                        else
                        {
                            color = "DarkChromeLow";
                        }
                        return Parse(element, $"Theme={color}/{value.Split('/')[1]}/{value.Split('/')[2]}");
                    }
                    else
                    {
                        return Parse(element, reference.Value);
                    }
                }
            }

            // Pick the color considering theme.
            var map = fullMap.ContainsKey(elementScope) ? fullMap[elementScope] : fullMap[""];
            string colorName;
            if (SystemParameters.HighContrast)
            {
                if (map.ContainsKey(""))
                {
                    colorName = map[""];
                }
                else if (map.ContainsKey("HighContrast"))
                {
                    colorName = map["HighContrast"];
                }
                else if (map.ContainsKey("Theme"))
                {
                    colorName = map["Theme"].Replace("{Theme}", "Light");
                }
                else if (map.ContainsKey("Light"))
                {
                    colorName = map["Light"];
                }
                else throw new NotImplementedException($"BrushValueParser: '{value}'");
            }
            else
            {
                if (map.ContainsKey(""))
                {
                    colorName = map[""];
                }
                else if (map.ContainsKey("Theme"))
                {
                    colorName = map["Theme"].Replace("{Theme}", isLight ? "Light" : "Dark");
                }
                else if (map.ContainsKey("Light") && isLight)
                {
                    colorName = map["Light"];
                }
                else if (map.ContainsKey("Dark") && !isLight)
                {
                    colorName = map["Dark"];
                }
                else throw new NotImplementedException($"BrushValueParser: '{value}'");
            }

            Color ret;
            colorName = ExtractColorName(colorName, out var opacity);
            if (colorName[0] == '#')
            {
                ret = (Color)ColorConverter.ConvertFromString(colorName);
            }
            // TODO: HighContrast colors not querieid here still.
            else if (!ImmersiveSystemColors.TryLookup($"Immersive{colorName}", out var color))
            {
                var info = typeof(Colors).GetProperty(colorName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                ret = (Color)info.GetValue(null, null);
            }
            else
            {
                ret = color;
            }

            if (opacity > 0)
            {
                ret.A = (byte)(opacity * 255);
            }
            return new SolidColorBrush(ret);
        }

        private static string ExtractColorName(string colorName, out double opacity)
        {
            double opacityWhenTransparent = 0;
            double opacityWhenNotTransparent = 0;

            if (colorName.Contains("/"))
            {
                var parts = colorName.Split('/');
                if (parts.Length == 2)
                {
                    opacityWhenTransparent = double.Parse(parts[1]);
                    colorName = parts[0];
                }
                else if (parts.Length == 3)
                {
                    opacityWhenTransparent = double.Parse(parts[1]);
                    opacityWhenNotTransparent = double.Parse(parts[2]);
                    colorName = parts[0];
                }
                else throw new NotImplementedException();
            }

            opacity = SystemSettings.IsTransparencyEnabled ? opacityWhenTransparent :
                 (opacityWhenNotTransparent > 0 ? opacityWhenNotTransparent : opacityWhenTransparent);
            return colorName;
        }
    }
}
