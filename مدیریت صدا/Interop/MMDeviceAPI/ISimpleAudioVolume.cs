using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface ISimpleAudioVolume
    {
        void SetMasterVolume(float fLevel, ref Guid EventContext);
        void GetMasterVolume(out float pfLevel);
        void SetMute(int bMute, ref Guid EventContext);
        int GetMute();
    }
}