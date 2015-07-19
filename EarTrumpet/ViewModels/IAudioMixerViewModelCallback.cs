using EarTrumpet.Models;

namespace EarTrumpet.ViewModels
{
    public interface IAudioMixerViewModelCallback
    {
        void SetVolume(EarTrumpetAudioSessionModel session, float volume);
    }
}
