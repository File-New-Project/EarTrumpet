using EarTrumpet.DataModel;
using EarTrumpet.Interop.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace EarTrumpet.UI.Themes
{
    class BrushValueParser
    {
        public static SolidColorBrush Parse(DependencyObject element, string value)
        {
            try
            {
                bool isLight = Options.GetSource(element) == Options.SourceKind.App ? SystemSettings.IsLightTheme : SystemSettings.IsSystemLightTheme;
                string lightOrDarkText = isLight ? "Light" : "Dark";
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

                // Search for Refs in either the elementScope of deault map.
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
                    if (FindReference(element, searchKey, out var outRef))
                    {
                        return outRef;
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
                    else
                    {
                        throw new NotImplementedException($"BrushValueParser: '{value}'");
                    }
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
                    else
                    {
                        throw new NotImplementedException($"BrushValueParser: '{value}'");
                    }
                }

                if (FindReference(element, colorName, out var reference))
                {
                    return reference;
                }

                // Lookup the color
                Color ret;
                colorName = ParseOpacityFromColor(colorName, out var opacity);
                if (colorName[0] == '#')
                {
                    ret = (Color)ColorConverter.ConvertFromString(colorName);
                }
                else if (!ImmersiveSystemColors.TryLookup($"Immersive{colorName}", out var color))
                {
                    var info = typeof(Colors).GetProperty(colorName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    if (info == null)
                    {
                        info = typeof(SystemColors).GetProperty(colorName + "Color", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    }

                    if (info != null)
                    {
                        ret = (Color)info.GetValue(null, null);
                    }
                    else
                    {
                        // We don't expect to be here but are seeing on some machines that the template loading fails here.
                        // But it will succeed when the element Loaded is called.
                        ret = Colors.HotPink;
                        Trace.WriteLine($"## BrushValueParser Parse FAILED ## '{value}' {element}");
                    }
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
            catch (Exception ex)
            {
                throw new Exception($"BrushValueParser Error: '{value}' {element}", ex);
            }
        }

        private static bool FindReference(DependencyObject element, string searchKey, out SolidColorBrush outRef)
        {
            bool isLight = Options.GetSource(element) == Options.SourceKind.App ? SystemSettings.IsLightTheme : SystemSettings.IsSystemLightTheme;
            var reference = Manager.Current.References.FirstOrDefault(r => r.Key == searchKey.Split('/')[0]);
            if (reference != null)
            {
                if (reference.Value != null)
                {
                    outRef = Parse(element, reference.Value);
                    return true;
                }
                else
                {
                    var tab = new Dictionary<Rule.Kind, bool>();
                    tab.Add(Rule.Kind.Any, true);
                    tab.Add(Rule.Kind.HighContrast, SystemParameters.HighContrast);
                    tab.Add(Rule.Kind.LightTheme, isLight);
                    tab.Add(Rule.Kind.Transparency, SystemSettings.IsTransparencyEnabled && !SystemParameters.HighContrast);
                    tab.Add(Rule.Kind.UseAccentColor, SystemSettings.UseAccentColor && !isLight && !SystemParameters.HighContrast);
                    tab.Add(Rule.Kind.UseAccentColorOnWindowBorders, SystemSettings.UseAccentColorOnWindowBorders);
                    tab.Add(Rule.Kind.AccentPolicySupportsTintColor, AccentPolicyLibrary.AccentPolicySupportsTintColor);

                    Func<List<Rule>, string> ParseRule = null;
                    ParseRule = ruleList =>
                    {
                        var ret = ruleList.Where(
                            rule => tab[rule.On]).Select(
                            rule => (rule.Value != null) ? rule.Value : ParseRule(rule.Rules)).FirstOrDefault();
                        return ret != null ? ret : throw new NotImplementedException("Rule type not found");
                    };

                    string opacities = "";
                    var oItems = searchKey.Split('/').Skip(1).ToArray();
                    if (oItems.Length == 0)
                    {
                        // Nothing to do.
                    }
                    else if (oItems.Length == 1)
                    {
                        opacities = $"/{oItems[0]}";
                    }
                    else
                    {
                        opacities = $"/{oItems[0]}/{oItems[1]}";
                    }
                    outRef = Parse(element, ParseRule(reference.Rules) + opacities);
                    return true;
                }
            }
            outRef = null;
            return false;
        }

        // ColorName
        // ColorName/0.0
        // ColorName/0.0/0.0
        private static string ParseOpacityFromColor(string colorName, out double desiredOpacity)
        {
            double opacity = 0;
            double opacityNoTransparency = 0;

            var parts = colorName.Split('/');
            if (parts.Length == 1)
            {
                // Nothing to do.
            }
            else if (parts.Length == 2)
            {
                opacity = double.Parse(parts[1], CultureInfo.InvariantCulture);
                colorName = parts[0];
            }
            else if (parts.Length == 3)
            {
                opacity = double.Parse(parts[1], CultureInfo.InvariantCulture);
                opacityNoTransparency = double.Parse(parts[2], CultureInfo.InvariantCulture);
                colorName = parts[0];
            }
            else
            {
                throw new NotImplementedException("bad number of /");
            }

            desiredOpacity = SystemSettings.IsTransparencyEnabled && !SystemParameters.HighContrast ? opacity : (opacityNoTransparency > 0 ? opacityNoTransparency : opacity);
            return colorName;
        }
    }
}