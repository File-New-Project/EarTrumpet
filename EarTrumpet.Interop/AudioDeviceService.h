#pragma once

namespace EarTrumpet
{
    namespace Interop
    {
        struct EarTrumpetAudioDevice
        {
            PWSTR Id;
            PWSTR DisplayName;
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
            CComPtr<IEarTrumpetVolumeCallback> _earTrumpetVolumeCallback;

            void CleanUpAudioDevices();
            HRESULT GetDeviceByDeviceId(PCWSTR deviceId, IMMDevice** device);
            HRESULT SetMuteBoolForDevice(PCWSTR deviceId, BOOL value);
            HRESULT GetPolicyConfigClient(IPolicyConfig** client);

        public:
            static CComObject<AudioDeviceService>* instance()
            {
                if (!__instance)
                {
                    if (SUCCEEDED(CComObject<AudioDeviceService>::CreateInstance(&__instance)))
                    {
                        __instance->AddRef();
                    }
                }
                return __instance;
            }

            HRESULT RegisterVolumeChangeCallback(IEarTrumpetVolumeCallback* callback);
            HRESULT GetAudioDevices(void** audioDevices);
            HRESULT GetAudioDeviceVolume(PCWSTR deviceId, float* volume);
            HRESULT SetAudioDeviceVolume(PCWSTR deviceId, float volume);
            HRESULT SetDefaultAudioDevice(PCWSTR deviceId);
            HRESULT MuteAudioDevice(PCWSTR deviceId);
            HRESULT UnmuteAudioDevice(PCWSTR deviceId);
            HRESULT RefreshAudioDevices();
            int GetAudioDeviceCount();

            // IEndpointNotificationCallback
            STDMETHODIMP OnVolumeChanged(float volume);
        };
    }
}
