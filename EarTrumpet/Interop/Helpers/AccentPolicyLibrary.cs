using EarTrumpet.Extensions;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;

namespace EarTrumpet.Interop.Helpers
{
    static class AccentPolicyLibrary
    {
        public static bool AccentPolicySupportsTintColor => Environment.OSVersion.IsAtLeast(OSVersions.RS4);

        private static void SetInternal(IntPtr handle, User32.AccentState accentState, bool showBorders, uint color)
        {
            var accent = new User32.AccentPolicy
            {
                GradientColor = color,
                AccentState = accentState,
                AccentFlags = (showBorders) ? User32.AccentFlags.DrawAllBorders : User32.AccentFlags.None,
            };

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new User32.WindowCompositionAttribData();
            data.Attribute = User32.WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            var ret = User32.SetWindowCompositionAttribute(handle, ref data);
            Debug.Assert(ret == 0 || ret == 1);

            Marshal.FreeHGlobal(accentPtr);
        }

        private static void Set(IntPtr handle, bool isEnabled, bool withBorders, uint color)
        {
            if (isEnabled)
            {
                SetInternal(handle, AccentPolicySupportsTintColor ? User32.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND : User32.AccentState.ACCENT_ENABLE_BLURBEHIND, withBorders, color);
            }
            else
            {
                SetInternal(handle, User32.AccentState.ACCENT_DISABLED, false, 0);
            }
        }

        public static void SetWindowBlur(Window window, bool isEnabled, bool enableBorders, Color color)
        {
            var hwnd = ((HwndSource)HwndSource.FromVisual(window)).Handle;
            Set(hwnd, isEnabled, enableBorders, color.ToABGR());
        }

        public static void SetWindowBlur(Popup popupWindow, bool isEnabled, bool enableBorders, Color color)
        {
            var hwnd = ((HwndSource)HwndSource.FromVisual(popupWindow.Child)).Handle;
            Set(hwnd, isEnabled, enableBorders, color.ToABGR());
        }
    }
}
