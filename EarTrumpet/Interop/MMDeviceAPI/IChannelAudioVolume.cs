using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    [Guid("1C158861-B533-4B30-B1CF-E853E51C59B8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IChannelAudioVolume
    {
        int GetChannelCount();
        [PreserveSig]
        HRESULT SetChannelVolume(int index, float level);
        [PreserveSig]
        HRESULT GetChannelVolume(int Index, out float level);
        [PreserveSig]
        HRESULT SetAllVolumes(int count, IntPtr afLevels);
        [PreserveSig]
        HRESULT GetAllVolumes(int count, IntPtr afLevels);
    }
}
