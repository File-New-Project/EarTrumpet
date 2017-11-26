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
            CComPtr<IControlChangeCallback> _callback;

        public:
            std::wstring DeviceId;

            // IControlChangeHandler
            HRESULT __stdcall RegisterVolumeChangedCallback(IControlChangeCallback* obj);

            // IAudioEndpointVolumeCallback
            HRESULT __stdcall OnNotify(PAUDIO_VOLUME_NOTIFICATION_DATA pNotify);
        };
    }
}