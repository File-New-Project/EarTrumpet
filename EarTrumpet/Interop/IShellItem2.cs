using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace EarTrumpet.Interop
{
    [Guid("7E9FB0D3-919F-4307-AB2E-9B1860310C93")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IShellItem2 : IShellItem
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object BindToHandler(IBindCtx pbc, [In] ref Guid bhid, [In] ref Guid riid);
        IShellItem GetParent();
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetDisplayName(SIGDN sigdnName);
        SFGAO GetAttributes(SFGAO sfgaoMask);
        int Compare(IShellItem psi, SICHINT hint);
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyStore(
            GPS flags,
            [In] ref Guid riid);
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyStoreWithCreateObject(
            GPS flags,
            [MarshalAs(UnmanagedType.IUnknown)] object punkCreateObject,   // factory for low-rights creation of type ICreateObject
            [In] ref Guid riid);
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyStoreForKeys(
            IntPtr rgKeys,
            uint cKeys,
            GPS flags,
            [In] ref Guid riid);
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyDescriptionList(
            IntPtr keyType,
            [In] ref Guid riid);
        void Update(IBindCtx pbc);
        void GetProperty(ref PROPERTYKEY key, [In, Out] PropVariant pv);
        Guid GetCLSID(ref PROPERTYKEY key);
        System.Runtime.InteropServices.ComTypes.FILETIME GetFileTime(ref PROPERTYKEY key);
        int GetInt32(ref PROPERTYKEY key);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetString(ref PROPERTYKEY key);
        uint GetUInt32(ref PROPERTYKEY key);
        ulong GetUInt64(ref PROPERTYKEY key);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetBool(ref PROPERTYKEY key);
    }
}