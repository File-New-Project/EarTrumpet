using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Com
{
    [Guid("C3B284D4-6D39-4359-B3CF-B56DDB3BB39C")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioVolumeDuckNotification
    {
        void OnVolumeDuckNotification(string sessionID, uint countCommunicationSessions);
        void OnVolumeUnduckNotification(string sessionID);
    }
}