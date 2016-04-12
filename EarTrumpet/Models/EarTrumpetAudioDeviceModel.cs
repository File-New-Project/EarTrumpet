using System.Runtime.InteropServices;

namespace EarTrumpet.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct EarTrumpetAudioDeviceModel
    {
        public string Id;
        public string DisplayName;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsDefault;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsMuted;
    }
}