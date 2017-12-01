using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Interfaces
{

    [StructLayout(LayoutKind.Sequential)]
    public struct PROPERTYKEY
    {
        public Guid fmtid;
        public UIntPtr pid;
    }


    [ComImport]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPropertyStore_Shim
    {
        [PreserveSig]
        int GetCount([Out] out uint cProps);
        [PreserveSig]
        int GetAt([In] uint iProp, out PROPERTYKEY pkey);
        [PreserveSig]
        int GetValue([In] ref PROPERTYKEY key, out PropVariant pv);
        [PreserveSig]
        int SetValue([In] ref PROPERTYKEY key, [In] ref object pv);
        [PreserveSig]
        int Commit();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PropVariant
    {
        public short variantType;
        public short Reserved1, Reserved2, Reserved3;
        public IntPtr pointerValue;
    }

    public static class PropertyStoreInterop
    {
        [DllImport("ole32.dll")]
        internal static extern int PropVariantClear(ref PropVariant pvar);
    }

    public enum STGM
    {
        STGM_READ = 0,
        // ...
    }
}