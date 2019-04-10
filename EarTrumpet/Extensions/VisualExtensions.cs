using System.Windows;
using System.Windows.Media;

namespace EarTrumpet.Extensions
{
    public static class VisualExtensions
    {
        public static Matrix CalculateDpiFactors(this Visual visual)
        {
            var mainWindowPresentationSource = PresentationSource.FromVisual(visual);
            return mainWindowPresentationSource == null ? new Matrix() { M11 = 1, M22 = 1 } : mainWindowPresentationSource.CompositionTarget.TransformToDevice;
        }

        public static double DpiHeightFactor(this Visual visual)
        {
            var m = CalculateDpiFactors(visual);
            return m.M22;
        }

        public static double DpiWidthFactor(this Visual visual)
        {
            var m = CalculateDpiFactors(visual);
            return m.M11;
        }
    }
}
