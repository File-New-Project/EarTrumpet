using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using EarTrumpet.Extensions;

namespace EarTrumpet.Interop.Helpers;

public static class AccentPolicyLibrary
{
    public static bool AccentPolicySupportsTintColor => Environment.OSVersion.IsAtLeast(OSVersions.RS4);

    private static void SetAccentPolicy(IntPtr handle, User32.AccentPolicy policy)
    {
        var accentStructSize = Marshal.SizeOf(policy);
        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
        Marshal.StructureToPtr(policy, accentPtr, false);

        var data = new User32.WindowCompositionAttribData
        {
            Attribute = User32.WindowCompositionAttribute.WCA_ACCENT_POLICY,
            SizeOfData = accentStructSize,
            Data = accentPtr
        };

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
        var targetVisual = visual;

        // We don't want the parent window showing a popup
        if (visual is Popup popup && popup.Child != null)
        {
            targetVisual = popup;
        }

        return PresentationSource.FromVisual(targetVisual) is HwndSource source ? source.Handle : IntPtr.Zero;
    }
}
