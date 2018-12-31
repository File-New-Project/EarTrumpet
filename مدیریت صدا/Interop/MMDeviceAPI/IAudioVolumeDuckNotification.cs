using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    [Guid("C3B284D4-6D39-4359-B3CF-B56DDB3BB39C")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioVolumeDuckNotification
    {
        void OnVolumeDuckNotification([MarshalAs(UnmanagedType.LPWStr)]string sessionID, uint countCommunicationSessions);
        void OnVolumeUnduckNotification([MarshalAs(UnmanagedType.LPWStr)]string sessionID);
    }
}