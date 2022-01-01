using EarTrumpet.Interop.MMDeviceAPI;
using System;
using WinRT;

namespace EarTrumpet.Interop.Helpers
{
    class AudioPolicyConfigFactoryImplFor21H2 : IAudioPolicyConfigFactory
    {
        private readonly IAudioPolicyConfigFactoryVariantFor21H2 _factory;

        internal AudioPolicyConfigFactoryImplFor21H2()
        {
            var iid = typeof(IAudioPolicyConfigFactoryVariantFor21H2).GUID;

            var classId = MarshalString.CreateMarshaler("Windows.Media.Internal.AudioPolicyConfig");
            Combase.RoGetActivationFactory(classId.GetAbi(), ref iid, out object factory);
            classId.Dispose();

            _factory = (IAudioPolicyConfigFactoryVariantFor21H2)factory;
        }

        public HRESULT ClearAllPersistedApplicationDefaultEndpoints()
        {
            return _factory.ClearAllPersistedApplicationDefaultEndpoints();
        }

        public HRESULT GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, out string deviceId)
        {
            var hr = _factory.GetPersistedDefaultAudioEndpoint(processId, flow, role, out IntPtr deviceIdPtr);
            deviceId = MarshalString.FromAbi(deviceIdPtr);
            return hr;
        }

        public HRESULT SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId)
        {
            return _factory.SetPersistedDefaultAudioEndpoint(processId, flow, role, deviceId);
        }
    }
}
