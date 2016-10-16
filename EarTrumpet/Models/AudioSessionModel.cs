using System;

namespace EarTrumpet.Models
{
    public class AudioSessionModel
    {
        public string DeviceId;
        public string DisplayName;
        public string IconPath;
        public Guid GroupingId;
        public uint SessionId;
        public uint ProcessId;
        public uint BackgroundColor;
        public float Volume;
        public bool IsDesktop;
        public bool IsMuted;
    }
}
