using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Com
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
    public interface IPropertyStore
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

    public static class PropertyStoreInterop
    {
        [DllImport("ole32.dll")]
        internal static extern int PropVariantClear(ref PropVariant pvar);
    }
}