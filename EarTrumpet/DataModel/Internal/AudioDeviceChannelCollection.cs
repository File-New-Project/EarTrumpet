using EarTrumpet.Interop.MMDeviceAPI;
using System.Collections.Generic;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.Internal
{
    class AudioDeviceChannelCollection
    {
        public List<AudioDeviceChannel> Channels { get; }

        private readonly Dispatcher _dispatcher;

        public AudioDeviceChannelCollection(IAudioEndpointVolume deviceVolume, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            var ret = new List<AudioDeviceChannel>();
            for (uint i = 0; i < deviceVolume.GetChannelCount(); i++)
            {
                ret.Add(new AudioDeviceChannel(deviceVolume, i));
            }
            Channels = ret;
        }

        public void OnNotify(AUDIO_VOLUME_NOTIFICATION_DATA pNotify)
        {
            for (var i = 0; i < pNotify.nChannels; i++)
            {
                Channels[i].OnNotify();
            }
        }
    }
}
