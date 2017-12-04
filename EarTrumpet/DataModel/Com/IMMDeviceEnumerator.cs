using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Com
{
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceEnumerator
    {
        void EnumAudioEndpoints(EDataFlow dataFlow, uint dwStateMask, out IMMDeviceCollection ppDevices);
        void GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);
        void GetDevice([MarshalAs(UnmanagedType.LPWStr)]string pwstrId, out IMMDevice ppDevice);
        void RegisterEndpointNotificationCallback(IMMNotificationClient pClient);
        void UnregisterEndpointNotificationCallback(IMMNotificationClient pClient);
    }
}