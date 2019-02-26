using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio.Internal;

namespace EarTrumpet.DataModel.WindowsAudio
{
    public class WindowsAudioFactory
    {
        static IAudioDeviceManager s_playbackDevices;
        static IAudioDeviceManager s_recordingDevices;

        public static IAudioDeviceManager Create(AudioDeviceKind kind)
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
