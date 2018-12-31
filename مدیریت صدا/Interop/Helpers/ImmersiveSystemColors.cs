using EarTrumpet.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace EarTrumpet.Interop.Helpers
{
    class ImmersiveSystemColors
    {
        internal static Color Lookup(string name)
        {
            var colorSet = Uxtheme.GetImmersiveUserColorSetPreference(false, false);
            var colorType = Uxtheme.GetImmersiveColorTypeFromName(name);
            var rawColor = Uxtheme.GetImmersiveColorFromColorSetEx(colorSet, colorType, false, 0);

            return rawColor.ToABGRColor();
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
                colors.Add(name, Lookup($"Immersive{name}"));
            }

            return colors;
        }
    }
}
