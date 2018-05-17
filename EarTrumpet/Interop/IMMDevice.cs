using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDevice
    {
        void Activate(ref Guid iid, uint dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.Interface)] out object ppInterface);
        void OpenPropertyStore(uint stgmAccess, [MarshalAs(UnmanagedType.Interface)] out IPropertyStore ppProperties);
        void GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
        void GetState(out uint pdwState);
    }

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