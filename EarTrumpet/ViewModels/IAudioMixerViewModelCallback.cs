using EarTrumpet.Models;

namespace EarTrumpet.ViewModels
{
    public interface IAudioMixerViewModelCallback
    {
        void SetVolume(AudioSessionModel session, float volume);
        void SetMute(AudioSessionModel session, bool isMuted);
        void SetDeviceVolume(AudioDeviceModel device, float volume);
        void SetDeviceMute(AudioDeviceModel device, bool isMuted);
    }
}
