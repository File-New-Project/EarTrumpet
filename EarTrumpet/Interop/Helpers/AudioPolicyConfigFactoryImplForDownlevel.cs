using EarTrumpet.Interop.MMDeviceAPI;
using System;
using WinRT;

namespace EarTrumpet.Interop.Helpers
{
    class AudioPolicyConfigFactoryImplForDownlevel : IAudioPolicyConfigFactory
    {
        private readonly IAudioPolicyConfigFactoryVariantForDownlevel _factory;

        internal AudioPolicyConfigFactoryImplForDownlevel()
        {
            var iid = typeof(IAudioPolicyConfigFactoryVariantForDownlevel).GUID;

            var classId = MarshalString.CreateMarshaler("Windows.Media.Internal.AudioPolicyConfig");
            Combase.RoGetActivationFactory(classId.GetAbi(), ref iid, out object factory);
            classId.Dispose();
            
            _factory = (IAudioPolicyConfigFactoryVariantForDownlevel)factory;
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
