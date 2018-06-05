using EarTrumpet.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EarTrumpet.Misc
{
    static class AccentPolicyLibrary
    {
        private static readonly uint _defaultTintBackgroundColor = 0x000000; // BGR Black
        private static readonly uint _defaultTintOpacity = 42;

        private static void SetInternal(IntPtr handle, User32.AccentState accentState, bool showBorders = false, uint tintOpacity = 0)
        {
            var accent = new User32.AccentPolicy
            {
                AccentState = accentState,
                AccentFlags = (showBorders) ? User32.AccentFlags.DrawAllBorders : User32.AccentFlags.None,
                GradientColor = (_defaultTintOpacity << 24) | (_defaultTintBackgroundColor & 0xFFFFFF)
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

        public static void Set(IntPtr handle, bool isEnabled, bool withBorders = false)
        {
            if (isEnabled)
            {
                SetInternal(handle, User32.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, withBorders, _defaultTintOpacity);
            }
            else
            {
                SetInternal(handle, User32.AccentState.ACCENT_DISABLED);
            }
        }
    }
}
