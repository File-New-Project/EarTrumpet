#pragma once

namespace EarTrumpet
{
    namespace Interop
    {
        struct EarTrumpetAudioDevice
        {
            LPWSTR Id;
            LPWSTR DisplayName;
            bool IsDefault;
            bool IsMuted;
        };

        class AudioDeviceService
        {
        private:
            static AudioDeviceService* __instance;
            std::vector<EarTrumpetAudioDevice> _devices;
            
            void CleanUpAudioDevices();
            HRESULT GetDeviceByDeviceId(PWSTR deviceId, IMMDevice** device);
            HRESULT SetMuteBoolForDevice(LPWSTR deviceId, BOOL value);
            HRESULT GetPolicyConfigClient(IPolicyConfig** client);

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
            HRESULT GetAudioDeviceVolume(LPWSTR deviceId, float* volume);
            HRESULT SetAudioDeviceVolume(LPWSTR deviceId, float volume);
            HRESULT SetDefaultAudioDevice(LPWSTR deviceId);
            HRESULT MuteAudioDevice(LPWSTR deviceId);
            HRESULT UnmuteAudioDevice(LPWSTR deviceId);
            HRESULT RefreshAudioDevices();
            int GetAudioDeviceCount();
        };
    }
}
