using System;
using EarTrumpet.Interop.MMDeviceAPI;
using Windows.Win32;
using Windows.Win32.Media.Audio;
using WinRT;

namespace EarTrumpet.Interop.Helpers;

internal class AudioPolicyConfigFactoryImplFor21H2 : IAudioPolicyConfigFactory
{
    private readonly IAudioPolicyConfigFactoryVariantFor21H2 _factory;

    internal AudioPolicyConfigFactoryImplFor21H2()
    {
        var iid = typeof(IAudioPolicyConfigFactoryVariantFor21H2).GUID;

        var className = "Windows.Media.Internal.AudioPolicyConfig";
        PInvoke.WindowsCreateString(className, (uint)className.Length, out var classNameString);
        PInvoke.RoGetActivationFactory(classNameString, iid, out var factory);

        _factory = (IAudioPolicyConfigFactoryVariantFor21H2)factory;
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
