using EarTrumpet.DataModel.Internal;
using EarTrumpet.UI.Services;

namespace EarTrumpet.DataModel
{
    public class DataModelFactory
    {
        static IAudioDeviceManager s_playbackDevices;
        static IAudioDeviceManager s_recordingDevices;

        public static IAudioDeviceManager CreateAudioDeviceManager(AudioDeviceKind kind)
        {
            if (kind == AudioDeviceKind.Playback)
            {
                if (s_playbackDevices == null)
                {
                    s_playbackDevices = new AudioDeviceManager(AudioDeviceKind.Playback);
                }
                return s_playbackDevices;
            }
            else
            {
                if (s_recordingDevices == null)
                {
                    s_recordingDevices = new AudioDeviceManager(AudioDeviceKind.Recording);
                }
                return s_recordingDevices;
            }
        }

        public static IAudioDeviceManager CreateNonSharedDeviceManager(AudioDeviceKind kind)
        {
            return new AudioDeviceManager(kind);
        }
    }
}
