using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet.UI.Behaviors
{
    public static class ScrollViewerEx
    {
        public static object GetScrollToVerticalOffset(DependencyObject obj) => (object)obj.GetValue(ScrollToVerticalOffsetProperty);
        public static void SetScrollToVerticalOffset(DependencyObject obj, object value) => obj.SetValue(ScrollToVerticalOffsetProperty, value);
        public static readonly DependencyProperty ScrollToVerticalOffsetProperty =
        DependencyProperty.RegisterAttached("ScrollToVerticalOffset", typeof(object), typeof(ScrollViewerEx), new PropertyMetadata(null, ScrollToVerticalOffsetChanged));
        private static void ScrollToVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ScrollViewer)d).ScrollToVerticalOffset(0);
        }

    }
}
