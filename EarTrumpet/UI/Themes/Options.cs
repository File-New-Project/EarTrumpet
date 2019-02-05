using System.Windows;

namespace EarTrumpet.UI.Themes
{
    class Options
    {
        public enum SourceKind
        {
            App, System
        }

        public static SourceKind? GetSource(DependencyObject obj) => (SourceKind?)obj.GetValue(SourceProperty);
        public static void SetSource(DependencyObject obj, SourceKind? value) => obj.SetValue(SourceProperty, value);
        public static readonly DependencyProperty SourceProperty =
        DependencyProperty.RegisterAttached("Source", typeof(SourceKind?), typeof(Options), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
    }
}
