using EarTrumpet.Services;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace EarTrumpet.Extensions
{
    public static class PopupExtensions
    {
        public static void SetWindowBlur(this Popup window, bool isBlur, bool showAllBorders = false)
        {
            var hwnd = ((HwndSource)HwndSource.FromVisual(window.Child)).Handle;
            AccentPolicyService.SetBlurPolicy(hwnd, isBlur, showAllBorders);
        }
    }
}
