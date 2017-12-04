using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Com
{
    [Guid("641DD20B-4D41-49CC-ABA3-174B9477BB08")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionNotification
    {
        void OnSessionCreated(IAudioSessionControl NewSession);
    }
}