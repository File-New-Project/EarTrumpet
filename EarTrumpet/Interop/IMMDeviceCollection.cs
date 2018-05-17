using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceCollection
    {
        void GetCount(out uint pcDevices);
        void Item(uint nDevice, [MarshalAs(UnmanagedType.Interface)] out IMMDevice ppDevice);
    }
}