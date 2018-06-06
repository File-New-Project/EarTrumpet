using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    [Guid("6E21E72B-B9B0-42AE-A686-983CF784EDCD")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IResourceMap
    {
        void GetUri(/*...*/);
        void GetSubtree(/*...*/);
        void GetString(/*...*/);
        void GetStringForcontext(/*...*/);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetFilePath([MarshalAs(UnmanagedType.LPWStr)]string file);

        /*...*/
    }
}
