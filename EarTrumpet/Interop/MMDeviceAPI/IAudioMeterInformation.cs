using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    [Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioMeterInformation
    {
        float GetPeakValue();
        void GetMeteringChannelCount(out uint pnChannelCount);
        void GetChannelsPeakValues(uint u32ChannelCount, out float afPeakValues);
        void QueryHardwareSupport(out uint pdwHardwareSupportMask);
    }
}