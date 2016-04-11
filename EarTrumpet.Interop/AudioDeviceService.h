#pragma once

namespace EarTrumpet
{
    namespace Interop
    {
        struct EarTrumpetAudioDevice
        {
			LPWSTR Id;
			LPWSTR DisplayName;
        };

        class AudioDeviceService
        {
        private:
            static AudioDeviceService* __instance;
            std::vector<EarTrumpetAudioDevice> _devices;
            
            void CleanUpAudioDevices();

        public:
            static AudioDeviceService* instance()
            {
                if (!__instance)
                {
                    __instance = new AudioDeviceService;
                }
                return __instance;
            }

            HRESULT GetAudioDevices(void** audioDevices);
			HRESULT SetDefaultAudioDevice(LPWSTR deviceId);
            HRESULT RefreshAudioDevices();
			int GetAudioDeviceCount();
        };
    }
}