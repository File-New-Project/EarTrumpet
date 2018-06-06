using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace EarTrumpet.Extensions
{
    public static class PopupExtensions
    {
        public static Matrix CalculateDpiFactors(this Popup window)
        {
            var mainWindowPresentationSource = PresentationSource.FromVisual(window);
            return mainWindowPresentationSource == null ? new Matrix() { M11 = 1, M22 = 1 } : mainWindowPresentationSource.CompositionTarget.TransformToDevice;
        }

        public static double DpiHeightFactor(this Popup window)
        {
            var m = CalculateDpiFactors(window);
            return m.M22;
        }

        public static double DpiWidthFactor(this Popup window)
        {
            var m = CalculateDpiFactors(window);
            return m.M11;
        }
    }
}
