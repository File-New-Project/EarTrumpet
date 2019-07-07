using System.Windows;
using System.Windows.Media;

namespace EarTrumpet.Extensions
{
    public static class VisualExtensions
    {
        private static Matrix CalculateDpi(this Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);
            return source == null ? new Matrix() { M11 = 1, M22 = 1 } : source.CompositionTarget.TransformToDevice;
        }

        public static double DpiY(this Visual visual) => CalculateDpi(visual).M22;
        public static double DpiX(this Visual visual) => CalculateDpi(visual).M11;
    }
}
