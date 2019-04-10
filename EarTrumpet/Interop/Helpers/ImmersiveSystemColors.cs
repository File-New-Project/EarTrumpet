﻿using EarTrumpet.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace EarTrumpet.Interop.Helpers
{
    class ImmersiveSystemColors
    {
        public static Color Lookup(string name)
        {
            TryLookup(name, out var ret);
            return ret;
        }

        internal static bool TryLookup(string name, out Color result)
        {
            result = default(Color);
            var colorSet = Uxtheme.GetImmersiveUserColorSetPreference(false, false);
            var colorType = Uxtheme.GetImmersiveColorTypeFromName(name);
            var rawColor = Uxtheme.GetImmersiveColorFromColorSetEx(colorSet, colorType, false, 0);


            result = rawColor.ToABGRColor();

            return (rawColor != 4294902015);
        }

        internal static IDictionary<string, Color> GetList()
        {
            var colors = new Dictionary<string, Color>();
            var colorSet = Uxtheme.GetImmersiveUserColorSetPreference(false, false);

            for (uint i = 0; ; i++)
            {
                var ptr = Uxtheme.GetImmersiveColorNamedTypeByIndex(i);
                if (ptr == IntPtr.Zero)
                    break;

                var name = Marshal.PtrToStringUni(Marshal.ReadIntPtr(ptr));
                TryLookup($"Immersive{name}", out var color);
                colors.Add(name, color);
            }

            return colors;
        }
    }
}
