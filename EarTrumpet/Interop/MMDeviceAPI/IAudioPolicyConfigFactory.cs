using System;
using Windows.Win32.Media.Audio;

namespace EarTrumpet.Interop.MMDeviceAPI;

public interface IAudioPolicyConfigFactory
{
    HRESULT SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId);
    HRESULT GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, out string deviceId);
    HRESULT ClearAllPersistedApplicationDefaultEndpoints();
}
