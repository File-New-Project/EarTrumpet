using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionControl
    {
        void GetState(out AudioSessionState pRetVal);
        void GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        void SetDisplayName([MarshalAs(UnmanagedType.LPWStr)]string Value, ref Guid EventContext);
        void GetIconPath([MarshalAs(UnmanagedType.LPWStr)]out string pRetVal);
        void SetIconPath([MarshalAs(UnmanagedType.LPWStr)]string Value, ref Guid EventContext);
        void GetGroupingParam(out Guid pRetVal);
        void SetGroupingParam(ref Guid Override, ref Guid EventContext);
        void RegisterAudioSessionNotification([MarshalAs(UnmanagedType.Interface)] IAudioSessionEvents NewNotifications);
        void UnregisterAudioSessionNotification([MarshalAs(UnmanagedType.Interface)] IAudioSessionEvents NewNotifications);
    }
}