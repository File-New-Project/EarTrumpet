using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    [Guid("1C158861-B533-4B30-B1CF-E853E51C59B8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IChannelAudioVolume
    {
        uint GetChannelCount();
        [PreserveSig]
        HRESULT SetChannelVolume(uint index, float level, ref Guid eventContent);
        [PreserveSig]
        HRESULT GetChannelVolume(uint Index, out float level);
        [PreserveSig]
        HRESULT SetAllVolumes(uint count, IntPtr afLevels, ref Guid eventContent);
        [PreserveSig]
        HRESULT GetAllVolumes(uint count, IntPtr afLevels);
    }
}
