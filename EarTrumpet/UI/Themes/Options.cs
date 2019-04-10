using System.Windows;

namespace EarTrumpet.UI.Themes
{
    public class Options
    {
        public enum SourceKind
        {
            App, System
        }

        public static SourceKind? GetSource(DependencyObject obj) => (SourceKind?)obj.GetValue(SourceProperty);
        public static void SetSource(DependencyObject obj, SourceKind? value) => obj.SetValue(SourceProperty, value);
        public static readonly DependencyProperty SourceProperty =
        DependencyProperty.RegisterAttached("Source", typeof(SourceKind?), typeof(Options), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static string GetScope(DependencyObject obj) => (string)obj.GetValue(ScopeProperty);
        public static void SetScope(DependencyObject obj, string value) => obj.SetValue(ScopeProperty, value);
        public static readonly DependencyProperty ScopeProperty =
        DependencyProperty.RegisterAttached("Scope", typeof(string), typeof(Options), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.Inherits));
    }
}
