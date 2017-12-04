using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Com
{
    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionManager2
    {
        void GetAudioSessionControl(ref Guid AudioSessionGuid, uint StreamFlags, out IAudioSessionControl SessionControl);
        void GetSimpleAudioVolume(ref Guid AudioSessionGuid, uint StreamFlags, out ISimpleAudioVolume AudioVolume);
        IAudioSessionEnumerator GetSessionEnumerator();
        void RegisterSessionNotification(IAudioSessionNotification SessionNotification);
        void UnregisterSessionNotification(IAudioSessionNotification SessionNotification);
        void RegisterDuckNotification(string sessionID, IAudioVolumeDuckNotification duckNotification);
        void UnregisterDuckNotification(IAudioVolumeDuckNotification duckNotification);
    }
}