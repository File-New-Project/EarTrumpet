using System.Windows;
using System.Windows.Media;

namespace EarTrumpet.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static T FindAnchestor<T>(this DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }
    }
}
