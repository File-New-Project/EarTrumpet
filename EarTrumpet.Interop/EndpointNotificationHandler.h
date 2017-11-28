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
            // IEndpointNotificationHandler
            STDMETHODIMP RegisterVolumeChangeHandler(IMMDeviceEnumerator* deviceEnumerator, IEndpointNotificationCallback* callback);

            // IMMNotificationClient
            STDMETHODIMP OnDeviceStateChanged(PCWSTR pwstrDeviceId, DWORD dwNewState);
            STDMETHODIMP OnDeviceAdded(PCWSTR pwstrDeviceId);
            STDMETHODIMP OnDeviceRemoved(PCWSTR pwstrDeviceId);
            STDMETHODIMP OnDefaultDeviceChanged(EDataFlow flow, ERole role, PCWSTR pwstrDefaultDeviceId);
            STDMETHODIMP OnPropertyValueChanged(PCWSTR pwstrDeviceId, const PROPERTYKEY key);

            // IControlChangeCallback
            STDMETHODIMP OnVolumeChanged(PCWSTR deviceId, float volume);

        private:
            std::map<std::wstring, CComPtr<IMMDevice>> _cachedDevices;
            std::map<std::wstring, CComPtr<IAudioEndpointVolume>> _cachedEndpoints;
            std::map<std::wstring, CComObject<ControlChangeHandler>*> _controlChangeHandlers;
            
            std::wstring _lastSeenDeviceId;
            CComPtr<IEndpointNotificationCallback> _endpointCallback;
            CComPtr<IMMDeviceEnumerator> _deviceEnumerator;

            HRESULT GetCachedDeviceByDeviceId(std::wstring const& deviceId, IMMDevice** device);
            HRESULT GetCachedAudioEndpointVolumeByDeviceId(std::wstring const& deviceId, IAudioEndpointVolume** audioEndpointVolume);
            HRESULT GetCachedControlChangeHandlerByDeviceId(std::wstring const& deviceId, CComObject<ControlChangeHandler>** controlChangeHandler);
            HRESULT GetDefaultDeviceVolume(float* volume);
        };
    }
}