﻿using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    [ComImport]
    [Guid("130A2F65-2BE7-4309-9A58-A9052FF2B61C")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMrtResourceManager
    {
        void Initialize();
        void InitializeForCurrentApplication();
        void InitializeForPackage([MarshalAs(UnmanagedType.LPWStr)]string package);
        void InitializeForFile(/*...*/);
        void GetMainResourceMap(ref Guid guid, [MarshalAs(UnmanagedType.Interface)]out IResourceMap resourceMap);

        /* ... */
    }

    static class IMrtResourceManagerExtensions
    {
        public static IResourceMap GetMainResourceMap(this IMrtResourceManager resourceManager)
        {
            Guid iid = typeof(IResourceMap).GUID;
            resourceManager.GetMainResourceMap(iid, out IResourceMap ret);
            return ret;
        }
    }
}
