#include "common.h"
#include "Mmdeviceapi.h"
#include "endpointvolume.h"

#include "IControlChangeCallback.h"
#include "IControlChangeHandler.h"
#include "ControlChangeHandler.h"

#include "IEndpointNotificationHandler.h"
#include "IEndpointNotificationCallback.h"
#include "EndpointNotificationHandler.h"

using namespace EarTrumpet::Interop;

EndpointNotificationHandler::~EndpointNotificationHandler()
{
    // TODO: Cleanup
}

HRESULT EndpointNotificationHandler::OnDefaultDeviceChanged(EDataFlow dataFlow, ERole role, PCWSTR deviceId)
{
    if (dataFlow == EDataFlow::eRender && role != ERole::eCommunications)
    {
        _currentDefaultDeviceHash = std::hash<std::wstring>()(deviceId);
        if (!_controlChangeHandlers.count(_currentDefaultDeviceHash))
        {
            IAudioEndpointVolume* audioEndpointVolume;
            FAST_FAIL(GetCachedAudioEndpointVolumeByDeviceId(deviceId, &audioEndpointVolume));

            CComObject<ControlChangeHandler>* controlChangeHandler;
            FAST_FAIL(GetCachedControlChangeHandlerByDeviceId(deviceId, &controlChangeHandler));
            controlChangeHandler->DeviceId = deviceId;
            controlChangeHandler->RegisterVolumeChangedCallback(this);
            FAST_FAIL(audioEndpointVolume->RegisterControlChangeNotify(controlChangeHandler));
        }
    }

    return S_OK;
}

HRESULT EndpointNotificationHandler::RegisterVolumeChangeHandler(IEndpointNotificationCallback* callback)
{
    _endpointCallback = callback;
    return S_OK;
}

HRESULT EndpointNotificationHandler::OnVolumeChanged(PCWSTR deviceId, float volume)
{
    // FIXME: Replace hashing

    auto deviceHash = std::hash<std::wstring>()(deviceId);
    if (deviceHash == _currentDefaultDeviceHash)
    {
        FAST_FAIL(_endpointCallback->OnVolumeChanged(volume));
    }

    return S_OK;
}

HRESULT EndpointNotificationHandler::GetCachedDeviceByDeviceId(PCWSTR deviceId, IMMDevice** device)
{
    auto deviceIdHash = std::hash<std::wstring>()(deviceId);
    if (!_cachedDevices.count(deviceIdHash))
    {
        CComPtr<IMMDeviceEnumerator> deviceEnumerator;
        FAST_FAIL(CoCreateInstance(__uuidof(MMDeviceEnumerator), nullptr, CLSCTX_INPROC, IID_PPV_ARGS(&deviceEnumerator)));
        FAST_FAIL(deviceEnumerator->GetDevice(deviceId, &_cachedDevices[deviceIdHash]));
    }

    *device = _cachedDevices[deviceIdHash];
    return S_OK;
}

HRESULT EndpointNotificationHandler::GetCachedAudioEndpointVolumeByDeviceId(PCWSTR deviceId, IAudioEndpointVolume** audioEndpointVolume)
{
    IMMDevice* device;
    FAST_FAIL(GetCachedDeviceByDeviceId(deviceId, &device));

    auto deviceIdHash = std::hash<std::wstring>()(deviceId);
    if (!_cachedEndpoints.count(deviceIdHash))
    {
        FAST_FAIL(device->Activate(__uuidof(IAudioEndpointVolume), CLSCTX_INPROC, nullptr, reinterpret_cast<void**>(&_cachedEndpoints[deviceIdHash])));
    }

    *audioEndpointVolume = _cachedEndpoints[deviceIdHash];
    return S_OK;
}

HRESULT EndpointNotificationHandler::GetCachedControlChangeHandlerByDeviceId(PCWSTR deviceId, CComObject<ControlChangeHandler>** controlChangeHandler)
{
    auto deviceIdHash = std::hash<std::wstring>()(deviceId);
    if (!_controlChangeHandlers.count(deviceIdHash))
    {
        FAST_FAIL(CComObject<ControlChangeHandler>::CreateInstance(&_controlChangeHandlers[deviceIdHash]));
        _controlChangeHandlers[deviceIdHash]->AddRef();
    }

    *controlChangeHandler = _controlChangeHandlers[deviceIdHash];
    return S_OK;
}

HRESULT EndpointNotificationHandler::OnDeviceStateChanged(PCWSTR, DWORD)
{
    return S_OK;
}

HRESULT EndpointNotificationHandler::OnDeviceAdded(PCWSTR)
{
    return S_OK;
}

HRESULT EndpointNotificationHandler::OnDeviceRemoved(PCWSTR)
{
    return S_OK;
}

HRESULT EndpointNotificationHandler::OnPropertyValueChanged(PCWSTR, const PROPERTYKEY)
{
    return S_OK;
}