using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    class Uxtheme
    {
        [DllImport("uxtheme.dll", EntryPoint = "#94", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern int GetImmersiveColorSetCount();

        [DllImport("uxtheme.dll", EntryPoint = "#95", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern uint GetImmersiveColorFromColorSetEx(
            uint dwImmersiveColorSet,
            uint dwImmersiveColorType,
            bool bIgnoreHighContrast,
            uint dwHighContrastCacheMode);

        [DllImport("uxtheme.dll", EntryPoint = "#96", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern uint GetImmersiveColorTypeFromName(
            string name);

        [DllImport("uxtheme.dll", EntryPoint = "#98", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern uint GetImmersiveUserColorSetPreference(
            bool bForceCheckRegistry,
            bool bSkipCheckOnFail);

        [DllImport("uxtheme.dll", EntryPoint = "#100", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern IntPtr GetImmersiveColorNamedTypeByIndex(
            uint dwIndex);
    }
}
