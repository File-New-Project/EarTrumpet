using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PROPERTYKEY
    {
        public Guid fmtid;
        public UIntPtr pid;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var pkey = ((PROPERTYKEY)obj);
            return pkey.fmtid == fmtid && pkey.pid == pid;
        }

        public override int GetHashCode()
        {
            return fmtid.GetHashCode() + pid.GetHashCode();
        }
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
        PropVariant GetValue([In] ref PROPERTYKEY key);
        [PreserveSig]
        int SetValue([In] ref PROPERTYKEY key, [In] ref PropVariant pv);
        [PreserveSig]
        int Commit();
    }
}