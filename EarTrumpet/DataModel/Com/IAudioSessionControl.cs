using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Com
{
    [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionControl
    {
        void GetState(out AudioSessionState pRetVal);
        void GetDisplayName(out string pRetVal);
        void SetDisplayName(string Value, ref Guid EventContext);
        void GetIconPath(out string pRetVal);
        void SetIconPath(string Value, ref Guid EventContext);
        void GetGroupingParam(out Guid pRetVal);
        void SetGroupingParam(ref Guid Override, ref Guid EventContext);
        void RegisterAudioSessionNotification(IAudioSessionEvents NewNotifications);
        void UnregisterAudioSessionNotification(IAudioSessionEvents NewNotifications);
    }
}