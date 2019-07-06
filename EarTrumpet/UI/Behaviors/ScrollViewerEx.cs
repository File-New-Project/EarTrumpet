using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet.UI.Behaviors
{
    public static class ScrollViewerEx
    {
        // ScrollToTopOnChanged: Scroll to top on any object change.
        public static object GetScrollToTopOnChanged(DependencyObject obj) => (object)obj.GetValue(ScrollToTopOnChangedProperty);
        public static void SetScrollToTopOnChanged(DependencyObject obj, object value) => obj.SetValue(ScrollToTopOnChangedProperty, value);
        public static readonly DependencyProperty ScrollToTopOnChangedProperty =
        DependencyProperty.RegisterAttached("ScrollToTopOnChanged", typeof(object), typeof(ScrollViewerEx), new PropertyMetadata(null, ScrollToTopOnChanged));
        private static void ScrollToTopOnChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((ScrollViewer)dependencyObject).ScrollToVerticalOffset(0);
        }
    }
}
