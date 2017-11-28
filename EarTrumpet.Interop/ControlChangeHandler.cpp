#include "common.h"
#include "Mmdeviceapi.h"
#include "endpointvolume.h"

#include "callbacks.h"
#include "handlers.h"
#include "ControlChangeHandler.h"

using namespace EarTrumpet::Interop;

HRESULT ControlChangeHandler::OnNotify(PAUDIO_VOLUME_NOTIFICATION_DATA pNotify)
{
    return _callback->OnVolumeChanged(_deviceId.c_str(), pNotify->fMasterVolume);
}

HRESULT ControlChangeHandler::RegisterVolumeChangedCallback(PCWSTR deviceId, IControlChangeCallback* callback)
{
    _deviceId = deviceId;
    _callback = callback;
    return S_OK;
}