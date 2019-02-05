using EarTrumpet.DataModel;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace EarTrumpet.UI.Themes
{
    public class Controller : BindableBase
    {
        public enum ThemeControllerKind
        {
            App,
            System
        }

        public ThemeControllerKind Kind { get; set; }

        public bool IsLightTheme => Kind == ThemeControllerKind.App ? SystemSettings.IsLightTheme : SystemSettings.IsSystemLightTheme;

        public Controller()
        {
            Manager.Current.ThemeChanged += () => RaisePropertyChanged(nameof(IsLightTheme));
        }

        private SolidColorBrush ResolveBrush(string light, string dark, string highContrast)
        {
            if (!string.IsNullOrWhiteSpace(highContrast) && SystemParameters.HighContrast)
            {
                // TODO
                return null;
            }
            else
            {
                bool isLight = Kind == ThemeControllerKind.App ? SystemSettings.IsLightTheme : SystemSettings.IsSystemLightTheme;

                // TODO: 19H1 compat line.
                if (Kind == ThemeControllerKind.System) isLight = false;

                var colorName = isLight ? light : dark;

                var reference = Manager.Current.References.FirstOrDefault(r => r.Key == colorName);
                if (reference != null)
                {
                    return ResolveBrush(reference.Value);
                }
                else if (colorName[0] == '#')
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorName));
                }
                else if (!ImmersiveSystemColors.TryLookup($"Immersive{colorName}", out var color))
                {
                    var info = typeof(Colors).GetProperty(colorName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    return new SolidColorBrush((Color)info.GetValue(null, null));
                }
                else
                {
                    return new SolidColorBrush(color);
                }
            }
        }

        public SolidColorBrush ResolveBrush(string newValue)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            map["Light"] = map["Dark"] = map["HighContrast"] = null;
            foreach (var pair in newValue.Split(','))
            {
                if (pair.Split('=').Length == 1)
                {
                    map["Theme"] = pair;
                }
                else
                {
                    map[pair.Trim().Split('=')[0]] = pair.Trim().Split('=')[1];
                }
            }

            if (map.ContainsKey("Theme"))
            {
                map["Light"] = map["Theme"].Replace("{Theme}", "Light");
                map["Dark"]  = map["Theme"].Replace("{Theme}", "Dark");
            }

            return ResolveBrush(map["Light"], map["Dark"], map["HighContrast"]);
        }
    }
}
