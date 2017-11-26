using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Services
{
    [Guid("68CDB936-6903-48E5-BB36-7EF434F28B61")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEarTrumpetVolumeCallback
    {
        void OnVolumeChanged(float volume);
    }
}
