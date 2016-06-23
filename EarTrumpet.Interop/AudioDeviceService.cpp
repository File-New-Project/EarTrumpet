#include "common.h"
#include "Mmdeviceapi.h"
#include "PolicyConfig.h"
#include "AudioDeviceService.h"
#include "Functiondiscoverykeys_devpkey.h"
#include "Propidl.h"
#include "Endpointvolume.h"

using namespace std;
using namespace std::tr1;
using namespace EarTrumpet::Interop;

AudioDeviceService* AudioDeviceService::__instance = nullptr;

void AudioDeviceService::CleanUpAudioDevices()
{
    for (auto device = _devices.begin(); device != _devices.end(); device++)
    {
        CoTaskMemFree(device->Id);
        CoTaskMemFree(device->DisplayName);
    }

    _devices.clear();
}

HRESULT AudioDeviceService::RefreshAudioDevices()
{
    CleanUpAudioDevices();

    CComPtr<IMMDeviceEnumerator> deviceEnumerator;
    FAST_FAIL(CoCreateInstance(__uuidof(MMDeviceEnumerator), nullptr, CLSCTX_INPROC, IID_PPV_ARGS(&deviceEnumerator)));

    CComPtr<IMMDeviceCollection> deviceCollection;
    FAST_FAIL(deviceEnumerator->EnumAudioEndpoints(EDataFlow::eRender, ERole::eMultimedia, &deviceCollection));

    CComPtr<IMMDevice> defaultDevice;
    FAST_FAIL(deviceEnumerator->GetDefaultAudioEndpoint(EDataFlow::eRender, ERole::eMultimedia, &defaultDevice));

    CComHeapPtr<wchar_t> defaultDeviceId;
    FAST_FAIL(defaultDevice->GetId(&defaultDeviceId));

    UINT numDevices;
    FAST_FAIL(deviceCollection->GetCount(&numDevices));

    for (UINT i = 0; i < numDevices; i++)
    {
        CComPtr<IMMDevice> device;
        if (FAILED(deviceCollection->Item(i, &device)))
        {
            continue;
        }

        CComHeapPtr<wchar_t> deviceId;
        FAST_FAIL(device->GetId(&deviceId));

        CComPtr<IPropertyStore> propertyStore;
        FAST_FAIL(device->OpenPropertyStore(STGM_READ, &propertyStore));

        PROPVARIANT friendlyName;
        PropVariantInit(&friendlyName);
        FAST_FAIL(propertyStore->GetValue(PKEY_Device_FriendlyName, &friendlyName));
        
        CComPtr<IAudioEndpointVolume> audioEndpointVol;
        FAST_FAIL(device->Activate(__uuidof(IAudioEndpointVolume), CLSCTX_INPROC, nullptr, reinterpret_cast<void**>(&audioEndpointVol)));

        BOOL isMuted;
        FAST_FAIL(audioEndpointVol->GetMute(&isMuted));

        EarTrumpetAudioDevice audioDevice = {};
        FAST_FAIL(SHStrDup(friendlyName.pwszVal, &audioDevice.DisplayName));
        FAST_FAIL(SHStrDup(deviceId, &audioDevice.Id));
        audioDevice.IsDefault = (wcscmp(defaultDeviceId, deviceId) == 0);
        audioDevice.IsMuted = !!isMuted;
        _devices.push_back(audioDevice);

        PropVariantClear(&friendlyName);
    }

    return S_OK;
}

HRESULT AudioDeviceService::SetDefaultAudioDevice(LPWSTR deviceId)
{
    CComPtr<IPolicyConfig> policyConfig;
    FAST_FAIL(GetPolicyConfigClient(&policyConfig));
    return policyConfig->SetDefaultEndpoint(deviceId, ERole::eMultimedia);
}

HRESULT AudioDeviceService::GetPolicyConfigClient(IPolicyConfig** client)
{
    //
    // The IPolicyConfig interface GUID keeps changing in Windows 10 for unknown reasons.
    // We attempt CoCreateInstance with known IIDs cover all Windows 10 scenarios.
    //
    // Pulled out of _ATL_INTMAP_ENTRYs found in AudioSes.dll
    // (i.e. AudioSes!ATL::CComObject<CPolicyConfigClient>::QueryInterface)
    //

    if (FAILED(CoCreateInstance(CLSID_PolicyConfigClient, nullptr, CLSCTX_INPROC, IID_IPolicyConfig_TH1, reinterpret_cast<LPVOID*>(client))))
    {
        if (FAILED(CoCreateInstance(CLSID_PolicyConfigClient, nullptr, CLSCTX_INPROC, IID_IPolicyConfig_TH2, reinterpret_cast<LPVOID*>(client))))
        {
            FAST_FAIL(CoCreateInstance(CLSID_PolicyConfigClient, nullptr, CLSCTX_INPROC, IID_IPolicyConfig_RS1, reinterpret_cast<LPVOID*>(client)));
        }
    }

    return S_OK;
}

HRESULT AudioDeviceService::GetDeviceByDeviceId(PWSTR deviceId, IMMDevice** device)
{
    CComPtr<IMMDeviceEnumerator> deviceEnumerator;
    FAST_FAIL(CoCreateInstance(__uuidof(MMDeviceEnumerator), nullptr, CLSCTX_INPROC, IID_PPV_ARGS(&deviceEnumerator)));

    return deviceEnumerator->GetDevice(deviceId, device);
}

HRESULT AudioDeviceService::GetAudioDeviceVolume(LPWSTR deviceId, float* volume)
{
    CComPtr<IMMDevice> device;
    FAST_FAIL(this->GetDeviceByDeviceId(deviceId, &device));

    CComPtr<IAudioEndpointVolume> audioEndpointVol;
    FAST_FAIL(device->Activate(__uuidof(IAudioEndpointVolume), CLSCTX_INPROC, nullptr, reinterpret_cast<void**>(&audioEndpointVol)));

    return audioEndpointVol->GetMasterVolumeLevelScalar(volume);
}

HRESULT AudioDeviceService::SetAudioDeviceVolume(LPWSTR deviceId, float volume)
{
    CComPtr<IMMDevice> device;
    FAST_FAIL(this->GetDeviceByDeviceId(deviceId, &device));

    CComPtr<IAudioEndpointVolume> audioEndpointVol;
    FAST_FAIL(device->Activate(__uuidof(IAudioEndpointVolume), CLSCTX_INPROC, nullptr, reinterpret_cast<void**>(&audioEndpointVol)));

    return audioEndpointVol->SetMasterVolumeLevelScalar(volume, nullptr);
}

HRESULT AudioDeviceService::GetAudioDevices(void** audioDevices)
{
    if (_devices.size() == 0)
    {
        return HRESULT_FROM_WIN32(ERROR_NO_MORE_ITEMS);
    }

    *audioDevices = &_devices[0];
    return S_OK;
}

HRESULT AudioDeviceService::SetMuteBoolForDevice(LPWSTR deviceId, BOOL value)
{
    CComPtr<IMMDevice> device;
    FAST_FAIL(this->GetDeviceByDeviceId(deviceId, &device));

    CComPtr<IAudioEndpointVolume> audioEndpointVol;
    FAST_FAIL(device->Activate(__uuidof(IAudioEndpointVolume), CLSCTX_INPROC, nullptr, reinterpret_cast<void**>(&audioEndpointVol)));

    return audioEndpointVol->SetMute(value, nullptr);
}

HRESULT AudioDeviceService::MuteAudioDevice(LPWSTR deviceId)
{
    return SetMuteBoolForDevice(deviceId, TRUE);
}

HRESULT AudioDeviceService::UnmuteAudioDevice(LPWSTR deviceId)
{
    return SetMuteBoolForDevice(deviceId, FALSE);
}

int AudioDeviceService::GetAudioDeviceCount()
{
    return _devices.size();
}
