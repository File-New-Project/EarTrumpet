using System.Collections.Generic;
using System.Windows.Threading;
using Windows.Win32.Media.Audio;

namespace EarTrumpet.DataModel.WindowsAudio.Internal;

class AudioDeviceSessionChannelCollection
{
    public List<AudioDeviceSessionChannel> Channels { get; }

    public AudioDeviceSessionChannelCollection(IChannelAudioVolume session, Dispatcher dispatcher)
    {
        var ret = new List<AudioDeviceSessionChannel>();
        session.GetChannelCount(out var channelCount);
        for (uint i = 0; i < channelCount; i++)
        {
            ret.Add(new AudioDeviceSessionChannel(session, i, dispatcher));
        }
        Channels = ret;
    }
}
