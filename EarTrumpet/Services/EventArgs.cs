using System;

namespace EarTrumpet.Services
{
    public sealed class VolumeChangedArgs : EventArgs
    {
        public uint SessionId { get; private set; }
        public float Volume { get; private set; }
        public bool Muted { get; private set; }

        public VolumeChangedArgs(uint sessionId, float volume, bool muted)
        {
            SessionId = sessionId;
            Volume = volume;
            Muted = muted;
        }
    }

    public sealed class DisplayNameChangedArgs : EventArgs
    {
        public uint SessionId { get; private set; }
        public string DisplayName { get; private set; }

        public DisplayNameChangedArgs(uint sessionId, string displayName)
        {
            SessionId = sessionId;
            DisplayName = displayName;
        }
    }

    public sealed class GroupingChangedArgs : EventArgs
    {
        public uint SessionId { get; private set; }
        public Guid GroupingId { get; private set; }

        public GroupingChangedArgs(uint sessionId, Guid groupingId)
        {
            SessionId = sessionId;
            GroupingId = groupingId;
        }
    }

    public sealed class IconChangedArgs : EventArgs
    {
        public uint SessionId { get; private set; }
        public string Icon { get; private set; }
        public IconChangedArgs(uint sessionId, string icon)
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
