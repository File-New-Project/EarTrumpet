using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceEnumerator
    {
        void EnumAudioEndpoints(EDataFlow dataFlow, DeviceState dwStateMask, [MarshalAs(UnmanagedType.Interface)] out IMMDeviceCollection ppDevices);
        void GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, [MarshalAs(UnmanagedType.Interface)] out IMMDevice ppEndpoint);
        void GetDevice([MarshalAs(UnmanagedType.LPWStr)]string pwstrId, [MarshalAs(UnmanagedType.Interface)] out IMMDevice ppDevice);
        void RegisterEndpointNotificationCallback([MarshalAs(UnmanagedType.Interface)] IMMNotificationClient pClient);
        void UnregisterEndpointNotificationCallback([MarshalAs(UnmanagedType.Interface)] IMMNotificationClient pClient);
    }
}