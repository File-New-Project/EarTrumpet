using System.Collections.Generic;
using System.Windows;

namespace EarTrumpet.UI.Themes
{
    public static class Brush
    {
        private static readonly Dictionary<string, Dictionary<DependencyObject, ThemeBindingInfo<System.Windows.Media.Brush>>> _bindingInfo = 
            new Dictionary<string, Dictionary<DependencyObject, ThemeBindingInfo<System.Windows.Media.Brush>>>();

        private static void ImplementPropertyChanged(string propertyName, DependencyObject dependencyObject, object newValue)
        {
            var value = (string)newValue;

            if (!_bindingInfo.ContainsKey(propertyName))
            {
                _bindingInfo[propertyName] = new Dictionary<DependencyObject, ThemeBindingInfo<System.Windows.Media.Brush>>();
            }

            var outgoing = _bindingInfo[propertyName].ContainsKey(dependencyObject) ? _bindingInfo[propertyName][dependencyObject] : null;
            if (outgoing != null)
            {
                _bindingInfo[propertyName][dependencyObject] = null;
                outgoing.Leaving();
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                _bindingInfo[propertyName][dependencyObject] = new ThemeBindingInfo<System.Windows.Media.Brush>(dependencyObject, value, propertyName, BrushValueParser.Parse);
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

        public static string GetCaretBrush(DependencyObject obj) => (string)obj.GetValue(CaretBrushProperty);
        public static void SetCaretBrush(DependencyObject obj, string value) => obj.SetValue(CaretBrushProperty, value);
        public static readonly DependencyProperty CaretBrushProperty =
        DependencyProperty.RegisterAttached("CaretBrush", typeof(string), typeof(Brush), new PropertyMetadata("", CaretBrushChanged));
        private static void CaretBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ImplementPropertyChanged("CaretBrush", d, e.NewValue);
    }
}
