using EarTrumpet.DataModel;
using Interop.MMDeviceAPI;
using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Extensions
{
    static class IMMDeviceExtensions
    {
        public static T Activate<T>(this IMMDevice device)
        {
            tag_inner_PROPVARIANT unused = default(tag_inner_PROPVARIANT);
            IntPtr ret;
            device.Activate(typeof(T).GUID, (uint)CLSCTX.CLSCTX_INPROC_SERVER, ref unused, out ret);
            return (T)Marshal.GetObjectForIUnknown(ret);
        }

        public static string GetId(this IMMDevice device)
        {
            string id;
            device.GetId(out id);
            return id;
        }
    }
}
