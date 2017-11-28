#pragma once

namespace EarTrumpet
{
    namespace Interop
    {
        class ControlChangeHandler :
            public CComObjectRootEx<CComSingleThreadModel>,
            public IControlChangeHandler,
            public IAudioEndpointVolumeCallback
        {
            BEGIN_COM_MAP(ControlChangeHandler)
                COM_INTERFACE_ENTRY(IControlChangeHandler)
                COM_INTERFACE_ENTRY(IAudioEndpointVolumeCallback)
            END_COM_MAP()

        private:
            std::wstring _deviceId;
            CComPtr<IControlChangeCallback> _callback;

        public:
            // IControlChangeHandler
            STDMETHODIMP RegisterVolumeChangedCallback(PCWSTR deviceId, IControlChangeCallback* callback);

            // IAudioEndpointVolumeCallback
            STDMETHODIMP OnNotify(PAUDIO_VOLUME_NOTIFICATION_DATA pNotify);
        };
    }
}