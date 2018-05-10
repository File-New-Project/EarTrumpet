using EarTrumpet.Services;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace EarTrumpet.Extensions
{
    static class BlurWindowExtensions
    {
        public static void EnableBlur(this Window window)
        {
            AccentPolicyService.EnableBlur(new WindowInteropHelper(window).Handle);
        }

        public static void DisableBlur(this Window window)
        {
            AccentPolicyService.DisableBlur(new WindowInteropHelper(window).Handle);
        }

        public static void EnableBlur(this Popup popup)
        {
            AccentPolicyService.EnableBlur(((HwndSource)HwndSource.FromVisual(popup.Child)).Handle);
        }

        public static void DisableBlur(this Popup popup)
        {
            AccentPolicyService.DisableBlur(((HwndSource)HwndSource.FromVisual(popup.Child)).Handle);
        }
    }
}
