using System;
using System.Diagnostics;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.Interop.MMDeviceAPI;
using Windows.Win32;
using Windows.Win32.Media.Audio;
using Windows.Win32.System.WinRT;

namespace EarTrumpet.DataModel.WindowsAudio.Internal;

internal class AudioPolicyConfig(EDataFlow flow)
{
    private const string DEVINTERFACE_AUDIO_RENDER = "#{e6327cad-dcec-4949-ae8a-991e976a79d2}";
    private const string DEVINTERFACE_AUDIO_CAPTURE = "#{2eef81be-33fa-4800-9670-1cd474972c3f}";
    private const string MMDEVAPI_TOKEN = @"\\?\SWD#MMDEVAPI#";

    private IAudioPolicyConfigFactory _sharedPolicyConfig;
    private readonly EDataFlow _flow = flow;

    private void EnsurePolicyConfig()
    {
        _sharedPolicyConfig ??= AudioPolicyConfigFactory.Create();
    }

    private string GenerateDeviceId(string deviceId)
    {
        return $"{MMDEVAPI_TOKEN}{deviceId}{(_flow == EDataFlow.eRender ? DEVINTERFACE_AUDIO_RENDER : DEVINTERFACE_AUDIO_CAPTURE)}";
    }

    private static string UnpackDeviceId(string deviceId)
    {
        if (deviceId.StartsWith(MMDEVAPI_TOKEN))
        {
            deviceId = deviceId.Remove(0, MMDEVAPI_TOKEN.Length);
        }

        if (deviceId.EndsWith(DEVINTERFACE_AUDIO_RENDER))
        {
            deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_RENDER.Length);
        }

        if (deviceId.EndsWith(DEVINTERFACE_AUDIO_CAPTURE))
        {
            deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_CAPTURE.Length);
        }

        return deviceId;
    }

    public void SetDefaultEndPoint(string deviceId, uint processId)
    {
        Trace.WriteLine($"AudioPolicyConfigService SetDefaultEndPoint {deviceId} {processId}");
        try
        {
            EnsurePolicyConfig();

            var hstring = (HSTRING)null;

            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                var str = GenerateDeviceId(deviceId);
                unsafe
                {
                    fixed(char* ptr = str)
                    {
                        PInvoke.WindowsCreateString(ptr, (uint)str.Length, &hstring);
                    }
                }
            }

            _sharedPolicyConfig.SetPersistedDefaultAudioEndpoint(processId, _flow, ERole.eMultimedia, hstring);
            _sharedPolicyConfig.SetPersistedDefaultAudioEndpoint(processId, _flow, ERole.eConsole, hstring);

            PInvoke.WindowsDeleteString(hstring);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"{ex}");
        }
    }

    public string GetDefaultEndPoint(uint processId)
    {
        try
        {
            EnsurePolicyConfig();

            _sharedPolicyConfig.GetPersistedDefaultAudioEndpoint(processId, _flow, ERole.eMultimedia | ERole.eConsole, out var deviceId);
            return UnpackDeviceId(deviceId);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"{ex}");
        }

        return null;
    }
}
