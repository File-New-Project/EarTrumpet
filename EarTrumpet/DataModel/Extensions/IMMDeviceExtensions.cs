using EarTrumpet.DataModel.Com;
using System;

namespace EarTrumpet.Extensions
{
    static class IMMDeviceExtensions
    {
        public static T Activate<T>(this IMMDevice device)
        {
            Guid iid = typeof(T).GUID;
            device.Activate(ref iid, (uint)CLSCTX.CLSCTX_INPROC_SERVER, IntPtr.Zero, out object ret);
            return (T)ret;
        }

        public static string GetId(this IMMDevice device)
        {
            string id;
            device.GetId(out id);
            return id;
        }
    }
}
