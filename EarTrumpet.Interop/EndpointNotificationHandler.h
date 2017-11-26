#pragma once

namespace EarTrumpet
{
    namespace Interop
    {
        class EndpointNotificationHandler : 
            public CComObjectRootEx<CComSingleThreadModel>,
            public IEndpointNotificationHandler,
            public IMMNotificationClient,
            public IControlChangeCallback
        {
            BEGIN_COM_MAP(EndpointNotificationHandler)
                COM_INTERFACE_ENTRY(IEndpointNotificationHandler)
                COM_INTERFACE_ENTRY(IMMNotificationClient)
                COM_INTERFACE_ENTRY(IControlChangeCallback)
            END_COM_MAP()

        public:
            ~EndpointNotificationHandler();

            // IEndpointNotificationHandler
            HRESULT __stdcall RegisterVolumeChangeHandler(IEndpointNotificationCallback* callback);

            // IMMNotificationClient
            HRESULT __stdcall OnDeviceStateChanged(PCWSTR pwstrDeviceId, DWORD dwNewState);
            HRESULT __stdcall OnDeviceAdded(PCWSTR pwstrDeviceId);
            HRESULT __stdcall OnDeviceRemoved(PCWSTR pwstrDeviceId);
            HRESULT __stdcall OnDefaultDeviceChanged(EDataFlow flow, ERole role, PCWSTR pwstrDefaultDeviceId);
            HRESULT __stdcall OnPropertyValueChanged(PCWSTR pwstrDeviceId, const PROPERTYKEY key);

            // IControlChangeCallback
            HRESULT __stdcall OnVolumeChanged(PCWSTR deviceId, float volume);

        private:
            std::map<size_t, IMMDevice*> _cachedDevices;
            std::map<size_t, IAudioEndpointVolume*> _cachedEndpoints;
            std::map<size_t, CComObject<ControlChangeHandler>*> _controlChangeHandlers;
            size_t _currentDefaultDeviceHash = 0;
            CComPtr<IEndpointNotificationCallback> _endpointCallback;

            HRESULT GetCachedDeviceByDeviceId(PCWSTR deviceId, IMMDevice** device);
            HRESULT GetCachedAudioEndpointVolumeByDeviceId(PCWSTR deviceId, IAudioEndpointVolume** audioEndpointVolume);
            HRESULT GetCachedControlChangeHandlerByDeviceId(PCWSTR deviceId, CComObject<ControlChangeHandler>** controlChangeHandler);
        };
    }
}