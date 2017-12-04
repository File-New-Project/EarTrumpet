using System;

namespace EarTrumpet.DataModel.Com
{
    public struct AUDIO_VOLUME_NOTIFICATION_DATA
    {
        public Guid guidEventContext;
        public int bMuted;
        public float fMasterVolume;
        public uint nChannels;
        public float[] afChannelVolumes;
    }
}