using System;

namespace EarTrumpet.Services
{
    public sealed class DeviceMuteChangedArgs : EventArgs
    {
        public string DeviceId { get; private set; }
        public float Volume { get; private set; }
        public bool Muted { get; private set; }

        public DeviceMuteChangedArgs(string deviceId, float volume, bool muted)
        {
            DeviceId = deviceId;
            Volume = volume;
            Muted = muted;
        }
    }

    public sealed class DeviceVolumeChangedArgs : EventArgs
    {
        public string DeviceId { get; private set; }
        public float Volume { get; private set; }
        public bool Muted { get; private set; }

        public DeviceVolumeChangedArgs(string deviceId, float volume, bool muted)
        {
            DeviceId = deviceId;
            Volume = volume;
            Muted = muted;
        }
    }

    public sealed class SessionVolumeChangedArgs : EventArgs
    {
        public uint SessionId { get; private set; }
        public float Volume { get; private set; }
        public bool Muted { get; private set; }

        public SessionVolumeChangedArgs(uint sessionId, float volume, bool muted)
        {
            SessionId = sessionId;
            Volume = volume;
            Muted = muted;
        }
    }

    public sealed class SessionDisplayNameChangedArgs : EventArgs
    {
        public uint SessionId { get; private set; }
        public string DisplayName { get; private set; }

        public SessionDisplayNameChangedArgs(uint sessionId, string displayName)
        {
            SessionId = sessionId;
            DisplayName = displayName;
        }
    }

    public sealed class SessionGroupingChangedArgs : EventArgs
    {
        public uint SessionId { get; private set; }
        public Guid GroupingId { get; private set; }

        public SessionGroupingChangedArgs(uint sessionId, Guid groupingId)
        {
            SessionId = sessionId;
            GroupingId = groupingId;
        }
    }

    public sealed class SessionIconChangedArgs : EventArgs
    {
        public uint SessionId { get; private set; }
        public string Icon { get; private set; }
        public SessionIconChangedArgs(uint sessionId, string icon)
        {
            SessionId = sessionId;
            Icon = icon;
        }
    }

    public sealed class SessionStateChangedArgs : EventArgs
    {
        public uint SessionId { get; private set; }
        public SessionStateChangedArgs(uint sessionId)
        {
            SessionId = sessionId;
        }
    }
}
