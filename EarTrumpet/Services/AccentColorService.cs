using EarTrumpet.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace EarTrumpet.Services
{
    public static class AccentColorService
    {
        public static Color GetColorByTypeName(string name)
        {
            var colorSet = Uxtheme.GetImmersiveUserColorSetPreference(false, false);
            var colorType = Uxtheme.GetImmersiveColorTypeFromName(name);
            var rawColor = Uxtheme.GetImmersiveColorFromColorSetEx(colorSet, colorType, false, 0);

            return FromABGR(rawColor);
        }

        public static Color FromABGR(uint abgrValue)
        {
            var colorBytes = new byte[4];
            colorBytes[0] = (byte)((0xFF000000 & abgrValue) >> 24);     // A
            colorBytes[1] = (byte)((0x00FF0000 & abgrValue) >> 16);     // B
            colorBytes[2] = (byte)((0x0000FF00 & abgrValue) >> 8);      // G
            colorBytes[3] = (byte)(0x000000FF & abgrValue);             // R

            return Color.FromArgb(colorBytes[0], colorBytes[3], colorBytes[2], colorBytes[1]);
        }

        public static IDictionary<string, Color> GetImmersiveColors()
        {
            var colors = new Dictionary<string, Color>();
            var colorSet = Uxtheme.GetImmersiveUserColorSetPreference(false, false);

            for (uint i = 0; ; i++)
            {
                var ptr = Uxtheme.GetImmersiveColorNamedTypeByIndex(i);
                if (ptr == IntPtr.Zero)
                    break;

                var name = Marshal.PtrToStringUni(Marshal.ReadIntPtr(ptr));
                colors.Add(name, GetColorByTypeName($"Immersive{name}"));
            }

            return colors;
        }
    }
}
