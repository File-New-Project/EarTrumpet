#include "common.h"
#include "Mmdeviceapi.h"
#include "endpointvolume.h"

#include "callbacks.h"
#include "handlers.h"
#include "ControlChangeHandler.h"
#include "EndpointNotificationHandler.h"

using namespace EarTrumpet::Interop;

HRESULT EndpointNotificationHandler::OnDefaultDeviceChanged(EDataFlow dataFlow, ERole role, PCWSTR rawDeviceId)
{
    if (dataFlow == EDataFlow::eRender && role != ERole::eCommunications)
    {
        if (rawDeviceId != nullptr)
        {
            std::wstring deviceId(rawDeviceId);
            _lastSeenDeviceId = deviceId;
            if (_controlChangeHandlers.find(deviceId) == _controlChangeHandlers.end())
            {
                ControlChangeHandler* controlChangeHandler;
                FAST_FAIL(GetCachedControlChangeHandlerByDeviceId(deviceId, &controlChangeHandler));
                controlChangeHandler->RegisterVolumeChangedCallback(deviceId.c_str(), this);

                IAudioEndpointVolume* audioEndpointVolume;
                FAST_FAIL(GetCachedAudioEndpointVolumeByDeviceId(deviceId, &audioEndpointVolume));
                FAST_FAIL(audioEndpointVolume->RegisterControlChangeNotify(controlChangeHandler));
            }

            float volume;
            FAST_FAIL(GetDefaultDeviceVolume(&volume));
            FAST_FAIL(OnVolumeChanged(deviceId.c_str(), volume));
        }
        else
        {
            FAST_FAIL(OnVolumeChanged(nullptr, 0.0f));
        }
    }

    return S_OK;
}

HRESULT EndpointNotificationHandler::RegisterVolumeChangeHandler(IMMDeviceEnumerator* deviceEnumerator, IEndpointNotificationCallback* callback)
{
    _deviceEnumerator = deviceEnumerator;
    _endpointCallback = callback;

    CComPtr<IMMDevice> defaultDevice;
    FAST_FAIL(deviceEnumerator->GetDefaultAudioEndpoint(EDataFlow::eRender, ERole::eMultimedia, &defaultDevice));
    
    CComHeapPtr<wchar_t> defaultDeviceId;
    FAST_FAIL(defaultDevice->GetId(&defaultDeviceId));

    return OnDefaultDeviceChanged(EDataFlow::eRender, ERole::eMultimedia, defaultDeviceId);
}

HRESULT EndpointNotificationHandler::OnVolumeChanged(PCWSTR deviceId, float volume)
{
    if (deviceId == nullptr || _lastSeenDeviceId == deviceId)
    {
        FAST_FAIL(_endpointCallback->OnVolumeChanged(volume));
    }

    return S_OK;
}

HRESULT EndpointNotificationHandler::GetDefaultDeviceVolume(float* volume)
{
    IAudioEndpointVolume* endpointVolume;
    FAST_FAIL(GetCachedAudioEndpointVolumeByDeviceId(_lastSeenDeviceId, &endpointVolume));
    return endpointVolume->GetMasterVolumeLevelScalar(volume);
}

HRESULT EndpointNotificationHandler::GetCachedDeviceByDeviceId(std::wstring const& deviceId, IMMDevice** device)
{
    if (_cachedDevices.find(deviceId) == _cachedDevices.end())
    {
        FAST_FAIL(_deviceEnumerator->GetDevice(deviceId.c_str(), &_cachedDevices[deviceId]));
    }

    *device = _cachedDevices[deviceId].p;
    return S_OK;
}

HRESULT EndpointNotificationHandler::GetCachedAudioEndpointVolumeByDeviceId(std::wstring const& deviceId, IAudioEndpointVolume** audioEndpointVolume)
{
    IMMDevice* device;
    FAST_FAIL(GetCachedDeviceByDeviceId(deviceId, &device));

    if (_cachedEndpoints.find(deviceId) == _cachedEndpoints.end())
    {
        FAST_FAIL(device->Activate(__uuidof(IAudioEndpointVolume), CLSCTX_INPROC_SERVER,
            nullptr, reinterpret_cast<void**>(&_cachedEndpoints[deviceId])));
    }

    *audioEndpointVolume = _cachedEndpoints[deviceId].p;
    return S_OK;
}

HRESULT EndpointNotificationHandler::GetCachedControlChangeHandlerByDeviceId(std::wstring const& deviceId, ControlChangeHandler** controlChangeHandler)
{
    if (_controlChangeHandlers.find(deviceId) == _controlChangeHandlers.end())
    {
        FAST_FAIL(CComObject<ControlChangeHandler>::CreateInstance(&_controlChangeHandlers[deviceId]));
        _controlChangeHandlers[deviceId]->AddRef();
    }

    *controlChangeHandler = _controlChangeHandlers[deviceId];
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