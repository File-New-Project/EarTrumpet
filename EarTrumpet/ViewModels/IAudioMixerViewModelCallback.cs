using EarTrumpet.Models;

namespace EarTrumpet.ViewModels
{
    public interface IAudioMixerViewModelCallback
    {
        void SetVolume(EarTrumpetAudioSessionModel session, float volume);
        void SetMute(EarTrumpetAudioSessionModel session, bool isMuted);
        void SetDeviceVolume(EarTrumpetAudioDeviceModel device, float volume);
        void SetDeviceMute(EarTrumpetAudioDeviceModel device, bool isMuted);
    }
}
