using System.Runtime.InteropServices;

namespace EarTrumpet.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct EarTrumpetAudioDeviceModel
    {
        public string Id;
        public string DisplayName;
        public bool IsDefault;
    }
}