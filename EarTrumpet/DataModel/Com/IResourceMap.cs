using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Com
{
    [Guid("6E21E72B-B9B0-42AE-A686-983CF784EDCD")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IResourceMap
    {
        void GetUri(/*...*/);
        void GetSubtree(/*...*/);
        void GetString(/*...*/);
        void GetStringForcontext(/*...*/);
        void GetFilePath([MarshalAs(UnmanagedType.LPWStr)]string file, [MarshalAs(UnmanagedType.LPWStr)]out string path);

        /*...*/
    }
}
