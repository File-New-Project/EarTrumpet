using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Com
{
    [Guid("657804FA-D6AD-4496-8A60-352752AF4F89")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioEndpointVolumeCallback
    {
        void OnNotify(ref AUDIO_VOLUME_NOTIFICATION_DATA pNotify);
    }
}