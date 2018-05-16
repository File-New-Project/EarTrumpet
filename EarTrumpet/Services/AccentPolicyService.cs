using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EarTrumpet.Services
{
    public class AccentPolicyService
    {
        static class Interop
        {
            [DllImport("user32.dll")]
            internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttribData data);

            [StructLayout(LayoutKind.Sequential)]
            internal struct WindowCompositionAttribData
            {
                public WindowCompositionAttribute Attribute;
                public IntPtr Data;
                public int SizeOfData;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct AccentPolicy
            {
                public AccentState AccentState;
                public AccentFlags AccentFlags;
                public uint GradientColor;
                public uint AnimationId;
            }

            [Flags]
            internal enum AccentFlags
            {
                // ...
                DrawLeftBorder = 0x20,
                DrawTopBorder = 0x40,
                DrawRightBorder = 0x80,
                DrawBottomBorder = 0x100,
                DrawAllBorders = (DrawLeftBorder | DrawTopBorder | DrawRightBorder | DrawBottomBorder)
                // ...
            }

            internal enum WindowCompositionAttribute
            {
                // ...
                WCA_ACCENT_POLICY = 19
                // ...
            }

            internal enum AccentState
            {
                ACCENT_DISABLED = 0,
                ACCENT_ENABLE_GRADIENT = 1,
                ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
                ACCENT_ENABLE_BLURBEHIND = 3,
                ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
                ACCENT_INVALID_STATE = 5
            }
        }

        private static uint _blurBackgroundColor = 0x000000; // BGR Black
        private static uint _blurOpacity = 42;

        private static void SetAccentPolicy(IntPtr handle, Interop.AccentState accentState, bool showAllBorders = false, uint blurOpacity = 0)
        {
            var accent = new Interop.AccentPolicy();
            accent.AccentState = accentState;
            accent.AccentFlags = GetAccentFlagsForTaskbarPosition(showAllBorders);
            accent.GradientColor = (blurOpacity << 24) | (_blurBackgroundColor & 0xFFFFFF);

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new Interop.WindowCompositionAttribData();
            data.Attribute = Interop.WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            var ret = Interop.SetWindowCompositionAttribute(handle, ref data);
            Debug.Assert(ret == 0 || ret == 1);

            Marshal.FreeHGlobal(accentPtr);
        }

        private static Interop.AccentFlags GetAccentFlagsForTaskbarPosition(bool showAllBorders = false)
        {
            var flags = Interop.AccentFlags.DrawAllBorders;

            if (showAllBorders)
            {
                return flags;
            }

            switch (TaskbarService.GetWinTaskbarState().TaskbarPosition)
            {
                case TaskbarPosition.Top:
                    flags &= ~Interop.AccentFlags.DrawTopBorder;
                    break;

                case TaskbarPosition.Bottom:
                    flags &= ~Interop.AccentFlags.DrawBottomBorder;
                    break;

                case TaskbarPosition.Left:
                    flags &= ~Interop.AccentFlags.DrawLeftBorder;
                    break;

                case TaskbarPosition.Right:
                    flags &= ~Interop.AccentFlags.DrawRightBorder;
                    break;
            }

            return flags;
        }

        public static void SetBlurPolicy(IntPtr handle, bool isBlur, bool showAllBorders = false)
        {
            if (isBlur)
            {
                SetAccentPolicy(handle, Interop.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, showAllBorders, _blurOpacity);
            }
            else
            {
                SetAccentPolicy(handle, Interop.AccentState.ACCENT_DISABLED);
            }
        }
    }
}
