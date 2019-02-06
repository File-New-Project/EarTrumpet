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
    public static class Brush
    {
        class BrushInfo
        {
            readonly string _propertyName;
            DependencyObject _element;
            DependencyObject _loadTarget;
            string _value;
            bool _attached;
            System.Windows.Media.Brush _initialBrush;

            public BrushInfo(DependencyObject element, string value, string propertyName)
            {
                _propertyName = propertyName;
                _element = element;
                _value = value;

                if (AttachToPropertyWithController())
                {
                    dynamic target = element;
                    while (target.Parent != null)
                    {
                        target = target.Parent;
                    }

                    if (target is FrameworkElement)
                    {
                        _loadTarget = (FrameworkElement)target;
                        ((FrameworkElement)target).Loaded += Element_Loaded;
                    }
                }
            }

            public void Leaving()
            {
                if (_loadTarget != null)
                {
                    if (_loadTarget is FrameworkContentElement)
                    {
                        ((FrameworkContentElement)_loadTarget).Loaded -= Element_Loaded;
                    }
                    else if (_loadTarget is FrameworkElement)
                    {
                        ((FrameworkElement)_loadTarget).Loaded -= Element_Loaded;
                    }
                }

                if (_attached)
                {
                    WriteProperty(_element, _propertyName, _initialBrush);
                }

                _element = null;
            }

            private void Element_Loaded(object sender, RoutedEventArgs e)
            {
                if (_loadTarget is FrameworkContentElement)
                {
                    ((FrameworkContentElement)_loadTarget).Loaded -= Element_Loaded;
                }
                else if (_loadTarget is FrameworkElement)
                {
                    ((FrameworkElement)_loadTarget).Loaded -= Element_Loaded;
                }

                _loadTarget = null;

                AttachToPropertyWithController();
            }

            public bool AttachToPropertyWithController()
            {
                var type = Options.GetSource(_element);
                if (type == null)
                {
                    return false;
                }

                _initialBrush = (System.Windows.Media.Brush)ReadProperty(_element, _propertyName);
                _attached = true;
                WriteProperty(_element, _propertyName, ResolveBrush((Options.SourceKind)type, _value));
                Manager.Current.ThemeChanged += () => WriteProperty(_element, _propertyName, ResolveBrush((Options.SourceKind)type, _value));
                return true;
            }

            object ReadProperty(object target, string propertyName)
            {
                var prop = target.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                return prop.GetValue(target);
            }

            void WriteProperty(object target, string propertyName, object value)
            {
                if (target == null)
                {
                    return;
                }
                var prop = target.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                prop.SetValue(target, value);
            }

            private SolidColorBrush ResolveBrush(Options.SourceKind kind, string light, string dark, string highContrast)
            {
                if (!string.IsNullOrWhiteSpace(highContrast) && SystemParameters.HighContrast)
                {
                    // TODO SystemColors lookup
                    return null;
                }
                else
                {
                    bool isLight = kind == Options.SourceKind.App ? SystemSettings.IsLightTheme : SystemSettings.IsSystemLightTheme;

                    // TODO: 19H1 compat line.
                    if (kind == Options.SourceKind.System) isLight = false;

                    var colorName = isLight ? light : dark;

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

                    Color ret;
                    var reference = Manager.Current.References.FirstOrDefault(r => r.Key == colorName);
                    if (reference != null)
                    {
                        if (reference is AcrylicBackgroundReference)
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
                            return ResolveBrush(kind, $"Theme={color}");
                        }
                        else if (reference is FlyoutBackgroundReference)
                        {
                            string color;
                            if (SystemParameters.HighContrast)
                            {
                                return ResolveBrush(kind, "Theme=ApplicationBackground");
                            }
                            else if (SystemSettings.UseAccentColor)
                            {
                                color = SystemSettings.IsTransparencyEnabled ? "SystemAccentDark2" : "SystemAccentDark1";
                            }
                            else
                            {
                                color = "DarkChromeLow";
                            }

                            return ResolveBrush(kind, $"Theme={color}/{opacityWhenTransparent}/{opacityWhenNotTransparent}");
                        }

                        return ResolveBrush(kind, reference.Value);
                    }
                    else if (colorName[0] == '#')
                    {
                        ret = (Color)ColorConverter.ConvertFromString(colorName);
                    }
                    else if (!ImmersiveSystemColors.TryLookup($"Immersive{colorName}", out var color))
                    {
                        var info = typeof(Colors).GetProperty(colorName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                        ret = (Color)info.GetValue(null, null);
                    }
                    else
                    {
                        ret = color;
                    }

                    var opacity = SystemSettings.IsTransparencyEnabled ? opacityWhenTransparent :
                        (opacityWhenNotTransparent > 0 ? opacityWhenNotTransparent : opacityWhenTransparent);

                    if (opacity > 0)
                    {
                        ret.A = (byte)(opacity * 255);
                    }

                    return new SolidColorBrush(ret);
                }
            }

            public SolidColorBrush ResolveBrush(Options.SourceKind kind, string newValue)
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
                    map["Dark"] = map["Theme"].Replace("{Theme}", "Dark");
                }

                return ResolveBrush(kind, map["Light"], map["Dark"], map["HighContrast"]);
            }
        }

        private static readonly Dictionary<string, Dictionary<DependencyObject, BrushInfo>> _bindingInfo = new Dictionary<string, Dictionary<DependencyObject, BrushInfo>>();

        private static void ImplementPropertyChanged(string propertyName, DependencyObject dependencyObject, object newValue)
        {
            var value = (string)newValue;

            if (!_bindingInfo.ContainsKey(propertyName))
            {
                _bindingInfo[propertyName] = new Dictionary<DependencyObject, BrushInfo>();
            }

            var outgoing = _bindingInfo[propertyName].ContainsKey(dependencyObject) ? _bindingInfo[propertyName][dependencyObject] : null;
            if (outgoing != null)
            {
                _bindingInfo[propertyName][dependencyObject] = null;
                outgoing.Leaving();
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                _bindingInfo[propertyName][dependencyObject] = new BrushInfo(dependencyObject, value, propertyName);
            }
        }

        public static string GetForeground(DependencyObject obj) => (string)obj.GetValue(ForegroundProperty);
        public static void SetForeground(DependencyObject obj, string value) => obj.SetValue(ForegroundProperty, value);
        public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.RegisterAttached("Foreground", typeof(string), typeof(Brush), new PropertyMetadata("", ForegroundChanged));
        private static void ForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ImplementPropertyChanged("Foreground", d, e.NewValue);

        public static string GetBackground(DependencyObject obj) => (string)obj.GetValue(BackgroundProperty);
        public static void SetBackground(DependencyObject obj, string value) => obj.SetValue(BackgroundProperty, value);
        public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.RegisterAttached("Background", typeof(string), typeof(Brush), new PropertyMetadata("", BackgroundChanged));
        private static void BackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ImplementPropertyChanged("Background", d, e.NewValue);

        public static string GetBorderBrush(DependencyObject obj) => (string)obj.GetValue(BorderBrushProperty);
        public static void SetBorderBrush(DependencyObject obj, string value) => obj.SetValue(BorderBrushProperty, value);
        public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.RegisterAttached("BorderBrush", typeof(string), typeof(Brush), new PropertyMetadata("", BorderBrushChanged));
        private static void BorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ImplementPropertyChanged("BorderBrush", d, e.NewValue);

        public static string GetStroke(DependencyObject obj) => (string)obj.GetValue(StrokeProperty);
        public static void SetStroke(DependencyObject obj, string value) => obj.SetValue(StrokeProperty, value);
        public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.RegisterAttached("Stroke", typeof(string), typeof(Brush), new PropertyMetadata("", StrokeChanged));
        private static void StrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ImplementPropertyChanged("Stroke", d, e.NewValue);

        public static string GetFill(DependencyObject obj) => (string)obj.GetValue(FillProperty);
        public static void SetFill(DependencyObject obj, string value) => obj.SetValue(FillProperty, value);
        public static readonly DependencyProperty FillProperty =
        DependencyProperty.RegisterAttached("Fill", typeof(string), typeof(Brush), new PropertyMetadata("", FillChanged));
        private static void FillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ImplementPropertyChanged("Fill", d, e.NewValue);

        public static string GetSelectionBrush(DependencyObject obj) => (string)obj.GetValue(SelectionBrushProperty);
        public static void SetSelectionBrush(DependencyObject obj, string value) => obj.SetValue(SelectionBrushProperty, value);
        public static readonly DependencyProperty SelectionBrushProperty =
        DependencyProperty.RegisterAttached("SelectionBrush", typeof(string), typeof(Brush), new PropertyMetadata("", SelectionBrushChanged));
        private static void SelectionBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ImplementPropertyChanged("SelectionBrush", d, e.NewValue);
    }
}
