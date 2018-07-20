using EarTrumpet.DataModel.Internal;

namespace EarTrumpet.DataModel
{
    class DataModelFactory
    {
        public static IAudioDeviceManager CreateAudioDeviceManager()
        {
            return new AudioDeviceManager(AudioDeviceKind.Playback);
        }
    }
}
