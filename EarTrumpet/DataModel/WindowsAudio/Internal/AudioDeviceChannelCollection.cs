using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace EarTrumpet.DataModel.WindowsAudio.Internal
{
    class AudioDeviceChannelCollection
    {
        public List<AudioDeviceChannel> Channels { get; }

        private readonly Dispatcher _dispatcher;
        private readonly int _afChannelVolumesOffset;

        public AudioDeviceChannelCollection(IAudioEndpointVolume deviceVolume, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            var ret = new List<AudioDeviceChannel>();
            for (uint i = 0; i < deviceVolume.GetChannelCount(); i++)
            {
                ret.Add(new AudioDeviceChannel(deviceVolume, i));
            }
            Channels = ret;

            AUDIO_VOLUME_NOTIFICATION_DATA dummy;
            _afChannelVolumesOffset = Marshal.OffsetOf<AUDIO_VOLUME_NOTIFICATION_DATA>(nameof(dummy.afChannelVolumes)).ToInt32();
        }

        public void OnNotify(IntPtr pNotify, AUDIO_VOLUME_NOTIFICATION_DATA data)
        {
            var channelVolumesValues = new float[data.nChannels];
            Marshal.Copy(IntPtr.Add(pNotify, _afChannelVolumesOffset), channelVolumesValues, 0, (int)data.nChannels);

            for (var i = 0; i < data.nChannels; i++)
            {
                Channels[i].OnNotify(channelVolumesValues[i]);
            }
        }
    }
}
