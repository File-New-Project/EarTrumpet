using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Diagnostics;

namespace EarTrumpet.DataModel.Internal.Services
{
    class AudioPolicyConfigService
    {
        private const string DEVINTERFACE_AUDIO_RENDER = "#{e6327cad-dcec-4949-ae8a-991e976a79d2}";
        private const string MMDEVAPI_TOKEN = @"\\?\SWD#MMDEVAPI#";

        private IAudioPolicyConfigFactory _sharedPolicyConfig;

        private void EnsurePolicyConfig()
        {
            if (_sharedPolicyConfig == null)
            {
                Guid iid = typeof(IAudioPolicyConfigFactory).GUID;
                Combase.RoGetActivationFactory("Windows.Media.Internal.AudioPolicyConfig", ref iid, out object factory);
                _sharedPolicyConfig = (IAudioPolicyConfigFactory)factory;
            }
        }

        private string GenerateDeviceId(string deviceId)
        {
            return $"{MMDEVAPI_TOKEN}{deviceId}{DEVINTERFACE_AUDIO_RENDER}";
        }

        private string UnpackDeviceId(string deviceId)
        {
            if (deviceId.StartsWith(MMDEVAPI_TOKEN)) deviceId = deviceId.Remove(0, MMDEVAPI_TOKEN.Length);
            if (deviceId.EndsWith(DEVINTERFACE_AUDIO_RENDER)) deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_RENDER.Length);
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

                _sharedPolicyConfig.SetPersistedDefaultAudioEndpoint((uint)processId, EDataFlow.eRender, ERole.eMultimedia, hstring);
                _sharedPolicyConfig.SetPersistedDefaultAudioEndpoint((uint)processId, EDataFlow.eRender, ERole.eConsole, hstring);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
            }
        }

        public string GetDefaultEndPoint(int processId)
        {
            try
            {
                EnsurePolicyConfig();

                _sharedPolicyConfig.GetPersistedDefaultAudioEndpoint((uint)processId, EDataFlow.eRender, ERole.eMultimedia | ERole.eConsole, out string deviceId);
                return UnpackDeviceId(deviceId);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
            }

            return null;
        }
    }
}
