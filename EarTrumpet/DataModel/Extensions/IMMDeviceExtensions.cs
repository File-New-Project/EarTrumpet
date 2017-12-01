using MMDeviceAPI_Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.Extensions
{
    static class IMMDeviceExtensions
    {
        public static T Activate<T>(this IMMDevice device)
        {
            tag_inner_PROPVARIANT unused = default(tag_inner_PROPVARIANT);
            IntPtr ret;
            device.Activate(typeof(T).GUID, 1 /* inproc server*/, ref unused, out ret);
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
