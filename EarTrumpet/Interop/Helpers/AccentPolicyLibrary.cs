using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace EarTrumpet.Interop.Helpers
{
    static class AccentPolicyLibrary
    {
        private static readonly bool _isRunningOnRs4OrHigher = Environment.OSVersion.Version.Build >= 17134;
        private static readonly uint _defaultTintBackgroundColor = 0x000000; // BGR Black
        private static readonly uint _defaultTintOpacity = 42;

        private static void SetInternal(IntPtr handle, User32.AccentState accentState, bool showBorders = false, uint tintOpacity = 0)
        {
            var accent = new User32.AccentPolicy
            {
                AccentState = accentState,
                AccentFlags = (showBorders) ? User32.AccentFlags.DrawAllBorders : User32.AccentFlags.None,
            };

            if (_isRunningOnRs4OrHigher)
            {
                accent.GradientColor = (_defaultTintOpacity << 24) | (_defaultTintBackgroundColor & 0xFFFFFF);
            }

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

        private static void Set(IntPtr handle, bool isEnabled, bool withBorders = false)
        {
            if (isEnabled)
            {
                SetInternal(handle, _isRunningOnRs4OrHigher ? User32.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND : User32.AccentState.ACCENT_ENABLE_BLURBEHIND, withBorders, _defaultTintOpacity);
            }
            else
            {
                SetInternal(handle, User32.AccentState.ACCENT_DISABLED);
            }
        }

        public static void SetWindowBlur(Window window, bool isEnabled, bool enableBorders = false)
        {
            var hwnd = ((HwndSource)HwndSource.FromVisual(window)).Handle;
            Set(hwnd, isEnabled, enableBorders);
        }

        public static void SetWindowBlur(Popup popupWindow, bool isEnabled, bool enableBorders = false)
        {
            var hwnd = ((HwndSource)HwndSource.FromVisual(popupWindow.Child)).Handle;
            Set(hwnd, isEnabled, enableBorders);
        }
    }
}
