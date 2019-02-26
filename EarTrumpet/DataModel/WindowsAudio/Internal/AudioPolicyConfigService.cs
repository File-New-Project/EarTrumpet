using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Diagnostics;

namespace EarTrumpet.DataModel.WindowsAudio.Internal
{
    class AudioPolicyConfig
    {
        private const string DEVINTERFACE_AUDIO_RENDER = "#{e6327cad-dcec-4949-ae8a-991e976a79d2}";
        private const string DEVINTERFACE_AUDIO_CAPTURE = "#{2eef81be-33fa-4800-9670-1cd474972c3f}";
        private const string MMDEVAPI_TOKEN = @"\\?\SWD#MMDEVAPI#";

        private IAudioPolicyConfigFactory _sharedPolicyConfig;
        private EDataFlow _flow;

        public AudioPolicyConfig(EDataFlow flow)
        {
            _flow = flow;
        }

        private void EnsurePolicyConfig()
        {
            if (_sharedPolicyConfig == null)
            {
                _sharedPolicyConfig = AudioPolicyConfigFactory.Create();
            }
        }

        private string GenerateDeviceId(string deviceId)
        {
            return $"{MMDEVAPI_TOKEN}{deviceId}{(_flow == EDataFlow.eRender ? DEVINTERFACE_AUDIO_RENDER : DEVINTERFACE_AUDIO_CAPTURE)}";
        }

        private string UnpackDeviceId(string deviceId)
        {
            if (deviceId.StartsWith(MMDEVAPI_TOKEN)) deviceId = deviceId.Remove(0, MMDEVAPI_TOKEN.Length);
            if (deviceId.EndsWith(DEVINTERFACE_AUDIO_RENDER)) deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_RENDER.Length);
            if (deviceId.EndsWith(DEVINTERFACE_AUDIO_CAPTURE)) deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_CAPTURE.Length);
            return deviceId;
        }

        public void SetDefaultEndPoint(string deviceId, int processId)
        {
            Trace.WriteLine($"AudioPolicyConfigService SetDefaultEndPoint {deviceId} {processId}");
            try
            {
                EnsurePolicyConfig();

                IntPtr hstring = IntPtr.Zero;

                if (!string.IsNullOrWhiteSpace(deviceId))
                {
                    var str = GenerateDeviceId(deviceId);
                    Combase.WindowsCreateString(str, (uint)str.Length, out hstring);
                }

                _sharedPolicyConfig.SetPersistedDefaultAudioEndpoint((uint)processId, _flow, ERole.eMultimedia, hstring);
                _sharedPolicyConfig.SetPersistedDefaultAudioEndpoint((uint)processId, _flow, ERole.eConsole, hstring);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{ex}");
            }
        }

        public string GetDefaultEndPoint(int processId)
        {
            try
            {
                EnsurePolicyConfig();

                _sharedPolicyConfig.GetPersistedDefaultAudioEndpoint((uint)processId, _flow, ERole.eMultimedia | ERole.eConsole, out string deviceId);
                return UnpackDeviceId(deviceId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{ex}");
            }

            return null;
        }
    }
}
