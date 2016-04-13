using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct EarTrumpetAudioSessionModel
    {
        public string DisplayName;
        public string IconPath;
        public Guid GroupingId;
        public uint SessionId;
        public uint ProcessId;
        public uint BackgroundColor;
        public float Volume;
        
        [MarshalAs(UnmanagedType.I1)]
        public bool IsDesktop;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsMuted;
    }
}
