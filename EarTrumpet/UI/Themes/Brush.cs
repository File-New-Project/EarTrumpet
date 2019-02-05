using System.Collections.Generic;
using System.Windows;

namespace EarTrumpet.UI.Themes
{
    public static class Brush
    {
        static readonly Dictionary<string, Dictionary<DependencyObject, PropertyInfo>> _map = new Dictionary<string, Dictionary<DependencyObject, PropertyInfo>>();

        static void ImplementPropertyChanged(string propertyName, DependencyObject d, object newValue)
        {
            var element = (FrameworkElement)d;
            var value = (string)newValue;

            if (!_map.ContainsKey(propertyName))
            {
                _map[propertyName] = new Dictionary<DependencyObject, PropertyInfo>();
            }

            Dictionary<DependencyObject, PropertyInfo> map = _map[propertyName];

            PropertyInfo outgoing = map.ContainsKey(d) ? map[d] : null;

            if (outgoing != null)
            {
                map[d] = null;
                outgoing.Leaving();
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                var info = new PropertyInfo(element, value, propertyName);
                map[d] = info;
            }
        }

        class PropertyInfo
        {
            readonly string _propertyName;
            readonly FrameworkElement _element;
            FrameworkElement _loadTarget;
            string _value;
            Controller _controller;
            System.Windows.Media.Brush _initialBrush;

            public PropertyInfo(FrameworkElement element, string value, string propertyName)
            {
                _propertyName = propertyName;
                _element = element;
                _value = value;

                try
                {
                    var controller = (Controller)element.FindResource("ThemeController");
                    ConfigureProperty(controller);
                }
                catch (ResourceReferenceKeyNotFoundException)
                {
                    var target = element;
                    while (target.Parent != null)
                    {
                        target = target.Parent as FrameworkElement;
                    }

                    _loadTarget = target;
                    target.Loaded += Element_Loaded;
                }
            }

            public void Leaving()
            {
                if (_loadTarget != null)
                {
                    _loadTarget.Loaded -= Element_Loaded;
                }

                if (_controller != null)
                {
                    _controller.PropertyChanged -= Controller_PropertyChanged;
                    Write(_initialBrush);
                }
            }

            private void Element_Loaded(object sender, RoutedEventArgs e)
            {
                _loadTarget.Loaded -= Element_Loaded;
                _loadTarget = null;

                var controller = (Controller)_element.FindResource("ThemeController");
                ConfigureProperty(controller);
            }

            internal void ConfigureProperty(Controller controller)
            {
                _controller = controller;
                _initialBrush = (System.Windows.Media.Brush)Read();
                Write(_controller.ResolveBrush(_value));
                controller.PropertyChanged += Controller_PropertyChanged;
            }

            private void Controller_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                Write(_controller.ResolveBrush(_value));
            }

            void Write(object value)
            {
                WriteProperty(_element, _propertyName, value);
            }

            object Read()
            {
                return ReadProperty(_element, _propertyName);
            }

            object ReadProperty(object target, string propertyName)
            {
                var prop = target.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                return prop.GetValue(target);
            }

            void WriteProperty(object target, string propertyName, object value)
            {
                var prop = target.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                prop.SetValue(target, value);
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
