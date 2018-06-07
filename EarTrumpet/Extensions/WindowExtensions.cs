using EarTrumpet.Interop;
using EarTrumpet.UI.Helpers;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace EarTrumpet.Extensions
{
    internal static class WindowExtensions
    {
        public static void Move(this Window window, double top, double left, double height, double width)
        {
            User32.SetWindowPos(new WindowInteropHelper(window).Handle, IntPtr.Zero, (int)left, (int)top, (int)width, (int)height, User32.SWP_NOZORDER);
        }

        public static void RaiseWindow(this Window window)
        {
            window.Topmost = true;
            window.Activate();
            window.Topmost = false;
        }

        public static void Cloak(this Window window, bool hide = true)
        {
            int attributeValue = hide ? 1 : 0;
            DwmApi.DwmSetWindowAttribute(new WindowInteropHelper(window).Handle, DwmApi.DWMA_CLOAK, ref attributeValue, Marshal.SizeOf(attributeValue));
        }

        public static Matrix CalculateDpiFactors(this Window window)
        {
            var mainWindowPresentationSource = PresentationSource.FromVisual(window);
            return mainWindowPresentationSource == null ? new Matrix() { M11 = 1, M22 = 1 } : mainWindowPresentationSource.CompositionTarget.TransformToDevice;
        }

        public static double DpiHeightFactor(this Window window)
        {
            var m = CalculateDpiFactors(window);
            return m.M22;
        }

        public static double DpiWidthFactor(this Window window)
        {
            var m = CalculateDpiFactors(window);
            return m.M11;
        }
    }
}
