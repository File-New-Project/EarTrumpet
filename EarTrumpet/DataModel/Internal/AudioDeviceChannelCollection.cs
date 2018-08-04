using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

        public void OnNotify(IntPtr pNotify, AUDIO_VOLUME_NOTIFICATION_DATA data)
        {
            var afChannelVolumesOffset = Marshal.OffsetOf<AUDIO_VOLUME_NOTIFICATION_DATA>(nameof(data.afChannelVolumes));
            var channelVolumesValues = new float[data.nChannels];
            Marshal.Copy(IntPtr.Add(pNotify, afChannelVolumesOffset.ToInt32()), channelVolumesValues, 0, (int)data.nChannels);

            for (var i = 0; i < data.nChannels; i++)
            {
                Channels[i].OnNotify(channelVolumesValues[i]);
            }
        }
    }
}
