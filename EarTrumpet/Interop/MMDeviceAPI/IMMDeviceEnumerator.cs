using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceEnumerator
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        IMMDeviceCollection EnumAudioEndpoints(EDataFlow dataFlow, DeviceState dwStateMask);
        [return: MarshalAs(UnmanagedType.Interface)]
        IMMDevice GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role);
        [return: MarshalAs(UnmanagedType.Interface)]
        IMMDevice GetDevice([MarshalAs(UnmanagedType.LPWStr)]string pwstrId);
        void RegisterEndpointNotificationCallback([MarshalAs(UnmanagedType.Interface)] IMMNotificationClient pClient);
        void UnregisterEndpointNotificationCallback([MarshalAs(UnmanagedType.Interface)] IMMNotificationClient pClient);
    }
}