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
    public static class AccentPolicyLibrary
    {
        public static bool AccentPolicySupportsTintColor => Environment.OSVersion.IsAtLeast(OSVersions.RS4);

        private static void SetAccentPolicy(IntPtr handle, User32.AccentPolicy policy)
        {
            var accentStructSize = Marshal.SizeOf(policy);
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(policy, accentPtr, false);

            var data = new User32.WindowCompositionAttribData();
            data.Attribute = User32.WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            var ret = User32.SetWindowCompositionAttribute(handle, ref data);
            Debug.Assert(ret == 0 || ret == 1);

            Marshal.FreeHGlobal(accentPtr);
        }

        public static void EnableAcrylic(Visual target, Color color, User32.AccentFlags flags)
        {
            SetAccentPolicy(HandleFromVisual(target),
                new User32.AccentPolicy
                {
                    AccentFlags = flags,
                    AccentState = AccentPolicySupportsTintColor ? User32.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND : User32.AccentState.ACCENT_ENABLE_BLURBEHIND,
                    GradientColor = color.ToABGR(),
                });
        }

        public static void DisableAcrylic(Visual target)
        {
            SetAccentPolicy(HandleFromVisual(target),
                new User32.AccentPolicy
                {
                    AccentState = User32.AccentState.ACCENT_DISABLED,
                });
        }

        private static IntPtr HandleFromVisual(Visual visual)
        {
            return ((HwndSource)PresentationSource.FromVisual(
                (visual is Popup) ? ((Popup)visual).Child : visual
                )).Handle;
        }
    }
}
