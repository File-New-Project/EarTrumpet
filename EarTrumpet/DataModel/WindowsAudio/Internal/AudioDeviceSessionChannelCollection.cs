using EarTrumpet.Interop.MMDeviceAPI;
using System.Collections.Generic;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.WindowsAudio.Internal
{
    class AudioDeviceSessionChannelCollection
    {
        public List<AudioDeviceSessionChannel> Channels { get; }

        public AudioDeviceSessionChannelCollection(IChannelAudioVolume session, Dispatcher dispatcher)
        {
            var ret = new List<AudioDeviceSessionChannel>();
            for(uint i = 0; i < session.GetChannelCount(); i++)
            {
                ret.Add(new AudioDeviceSessionChannel(session, i, dispatcher));
            }
            Channels = ret;
        }
    }
}
