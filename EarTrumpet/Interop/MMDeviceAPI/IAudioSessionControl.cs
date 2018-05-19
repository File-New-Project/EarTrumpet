using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionControl
    {
        AudioSessionState GetState();
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetDisplayName();
        void SetDisplayName([MarshalAs(UnmanagedType.LPWStr)]string Value, ref Guid EventContext);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetIconPath();
        void SetIconPath([MarshalAs(UnmanagedType.LPWStr)]string Value, ref Guid EventContext);
        Guid GetGroupingParam();
        void SetGroupingParam(ref Guid Override, ref Guid EventContext);
        void RegisterAudioSessionNotification([MarshalAs(UnmanagedType.Interface)] IAudioSessionEvents NewNotifications);
        void UnregisterAudioSessionNotification([MarshalAs(UnmanagedType.Interface)] IAudioSessionEvents NewNotifications);
    }
}