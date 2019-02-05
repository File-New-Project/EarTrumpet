using EarTrumpet.DataModel;
using EarTrumpet.Interop.Helpers;
using System.Collections.Generic;
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

                try
                {
                    AttachToPropertyWithController();
                }
                catch (ResourceReferenceKeyNotFoundException)
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

            internal void AttachToPropertyWithController()
            {
                var type = Options.GetSource(_element);
                if (type == null) throw new ResourceReferenceKeyNotFoundException();

                _initialBrush = (System.Windows.Media.Brush)ReadProperty(_element, _propertyName);
                _attached = true;
                WriteProperty(_element, _propertyName, ResolveBrush((Options.SourceKind)type, _value));
                Manager.Current.ThemeChanged += () => WriteProperty(_element, _propertyName, ResolveBrush((Options.SourceKind)type, _value));
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



                    var reference = Manager.Current.References.FirstOrDefault(r => r.Key == colorName);
                    if (reference != null)
                    {
                        return ResolveBrush(kind, reference.Value);
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
    }
}
