using System;
using EarTrumpet.Interop.MMDeviceAPI;
using Windows.Win32;
using Windows.Win32.Media.Audio;
using WinRT;

namespace EarTrumpet.Interop.Helpers;

internal class AudioPolicyConfigFactoryImplForDownlevel : IAudioPolicyConfigFactory
{
    private readonly IAudioPolicyConfigFactoryVariantForDownlevel _factory;

    internal AudioPolicyConfigFactoryImplForDownlevel()
    {
        var iid = typeof(IAudioPolicyConfigFactoryVariantForDownlevel).GUID;

        var className = "Windows.Media.Internal.AudioPolicyConfig";
        PInvoke.WindowsCreateString(className, (uint)className.Length, out var classNameString);
        PInvoke.RoGetActivationFactory(classNameString, iid, out var factory);

        _factory = (IAudioPolicyConfigFactoryVariantForDownlevel)factory;
    }

    public HRESULT ClearAllPersistedApplicationDefaultEndpoints()
    {
        return _factory.ClearAllPersistedApplicationDefaultEndpoints();
    }

    public HRESULT GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, out string deviceId)
    {
        var hr = _factory.GetPersistedDefaultAudioEndpoint(processId, flow, role, out var deviceIdPtr);
        deviceId = MarshalString.FromAbi(deviceIdPtr);
        return hr;
    }

    public HRESULT SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId)
    {
        return _factory.SetPersistedDefaultAudioEndpoint(processId, flow, role, deviceId);
    }
}
