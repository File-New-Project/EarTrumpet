using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    [Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioMeterInformation
    {
        float GetPeakValue();
        uint GetMeteringChannelCount();
        [PreserveSig]
        HRESULT GetChannelsPeakValues(uint u32ChannelCount, IntPtr afPeakValues);
        void QueryHardwareSupport(out uint pdwHardwareSupportMask);
    }
}