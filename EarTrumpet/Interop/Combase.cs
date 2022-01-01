using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    static class Combase
    {
        [DllImport("combase.dll", PreserveSig = false)]
        public static extern void RoGetActivationFactory(
            IntPtr activatableClassId,
            [In] ref Guid iid,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object factory);

        [DllImport("combase.dll", PreserveSig = false)]
        public static extern void WindowsCreateString(
            [MarshalAs(UnmanagedType.LPWStr)] string src,
            [In] uint length,
            [Out] out IntPtr hstring);
    }
}
