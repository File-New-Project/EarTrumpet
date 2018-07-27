using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    [Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionControl2
    {
        void GetState(out AudioSessionState pRetVal);
        void GetDisplayName([MarshalAs(UnmanagedType.LPWStr)]out string pRetVal);
        void SetDisplayName([MarshalAs(UnmanagedType.LPWStr)]string Value, ref Guid EventContext);
        void GetIconPath([MarshalAs(UnmanagedType.LPWStr)]out string pRetVal);
        void SetIconPath([MarshalAs(UnmanagedType.LPWStr)]string Value, ref Guid EventContext);
        void GetGroupingParam(out Guid pRetVal);
        void SetGroupingParam(ref Guid Override, ref Guid EventContext);
        void RegisterAudioSessionNotification([MarshalAs(UnmanagedType.Interface)] IAudioSessionEvents NewNotifications);
        void UnregisterAudioSessionNotification([MarshalAs(UnmanagedType.Interface)] IAudioSessionEvents NewNotifications);
        void GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)]out string pRetVal);
        void GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)]out string pRetVal);
        [PreserveSig]
        int GetProcessId(out uint processId);
        [PreserveSig]
        HRESULT IsSystemSoundsSession();
        void SetDuckingPreference(int optOut);
    }
}