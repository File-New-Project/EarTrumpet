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

        class AudioDeviceService :
            public CComObjectRootEx<CComSingleThreadModel>,
            public IEndpointNotificationCallback
        {
            BEGIN_COM_MAP(AudioDeviceService)
                COM_INTERFACE_ENTRY(IEndpointNotificationCallback)
            END_COM_MAP()

        private:
            static CComObject<AudioDeviceService>* __instance;
            std::vector<EarTrumpetAudioDevice> _devices;
            CComPtr<IMMNotificationClient> _deviceNotificationClient;
            CComPtr<IEarTrumpetVolumeCallback> _earTrumpetVolumeCallback;

            void CleanUpAudioDevices();
            HRESULT GetDeviceByDeviceId(PWSTR deviceId, IMMDevice** device);
            HRESULT SetMuteBoolForDevice(LPWSTR deviceId, BOOL value);
            HRESULT GetPolicyConfigClient(IPolicyConfig** client);

        public:
            static CComObject<AudioDeviceService>* instance()
            {
                if (!__instance)
                {
                    if (SUCCEEDED(CComObject<AudioDeviceService>::CreateInstance(&__instance)))
                    {
                        __instance->AddRef();
                        // TODO: Release at dtor?
                    }
                }
                return __instance;
            }

            HRESULT RegisterVolumeChangeCallback(IEarTrumpetVolumeCallback* callback);
            HRESULT GetAudioDevices(void** audioDevices);
            HRESULT GetAudioDeviceVolume(LPWSTR deviceId, float* volume);
            HRESULT SetAudioDeviceVolume(LPWSTR deviceId, float volume);
            HRESULT SetDefaultAudioDevice(LPWSTR deviceId);
            HRESULT MuteAudioDevice(LPWSTR deviceId);
            HRESULT UnmuteAudioDevice(LPWSTR deviceId);
            HRESULT RefreshAudioDevices();
            int GetAudioDeviceCount();

            // IEndpointNotificationCallback
            HRESULT __stdcall OnVolumeChanged(float volume);
        };
    }
}
