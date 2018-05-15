using EarTrumpet.Services;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace EarTrumpet.Extensions
{
    internal static class WindowExtensions
    {
        static class Interop
        {
            [DllImport("dwmapi.dll", PreserveSig = true)]
            public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(
                IntPtr hWnd,
                IntPtr hWndInsertAfter,
                int X,
                int Y,
                int cx,
                int cy,
                uint uFlags);

            public const UInt32 SWP_NOSIZE = 0x0001;
            public const UInt32 SWP_NOMOVE = 0x0002;
            public const UInt32 SWP_NOZORDER = 0x0004;

            public const int DWMA_CLOAK = 0xD;
        }

        public static void Move(this Window window, double top, double left, double height, double width)
        {
            Interop.SetWindowPos(new WindowInteropHelper(window).Handle, IntPtr.Zero, (int)left, (int)top, (int)width, (int)height, Interop.SWP_NOZORDER);
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
            int ret = Interop.DwmSetWindowAttribute(new WindowInteropHelper(window).Handle, Interop.DWMA_CLOAK, ref attributeValue, Marshal.SizeOf(attributeValue));
            Debug.Assert(ret == 0);
        }

        public static void SetWindowBlur(this Window window, bool isBlur)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            AccentPolicyService.SetBlurPolicy(hwnd, isBlur);
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
