using EarTrumpet.Interop;
using EarTrumpet.Misc;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EarTrumpet.Services
{
    class AccentPolicyService
    {
        private static readonly uint _blurBackgroundColor = 0x000000; // BGR Black
        private static readonly uint _blurOpacity = 42;

        private static void SetAccentPolicy(IntPtr handle, User32.AccentState accentState, bool showAllBorders = false, uint blurOpacity = 0)
        {
            var accent = new User32.AccentPolicy
            {
                AccentState = accentState,
                AccentFlags = GetAccentFlagsForTaskbarPosition(showAllBorders),
                GradientColor = (blurOpacity << 24) | (_blurBackgroundColor & 0xFFFFFF)
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

        private static User32.AccentFlags GetAccentFlagsForTaskbarPosition(bool showAllBorders = false)
        {
            var flags = User32.AccentFlags.DrawAllBorders;

            if (showAllBorders)
            {
                return flags;
            }

            switch (WindowsTaskbar.Current.Location)
            {
                case WindowsTaskbar.Position.Top:
                    flags &= ~User32.AccentFlags.DrawTopBorder;
                    break;

                case WindowsTaskbar.Position.Bottom:
                    flags &= ~User32.AccentFlags.DrawBottomBorder;
                    break;

                case WindowsTaskbar.Position.Left:
                    flags &= ~User32.AccentFlags.DrawLeftBorder;
                    break;

                case WindowsTaskbar.Position.Right:
                    flags &= ~User32.AccentFlags.DrawRightBorder;
                    break;
            }

            return flags;
        }

        public static void SetBlurPolicy(IntPtr handle, bool isBlur, bool showAllBorders = false)
        {
            if (isBlur)
            {
                SetAccentPolicy(handle, User32.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, showAllBorders, _blurOpacity);
            }
            else
            {
                SetAccentPolicy(handle, User32.AccentState.ACCENT_DISABLED);
            }
        }
    }
}
