#include "common.h"
#include "Mmdeviceapi.h"
#include "endpointvolume.h"

#include "IControlChangeCallback.h"
#include "IControlChangeHandler.h"
#include "ControlChangeHandler.h"

using namespace EarTrumpet::Interop;

HRESULT ControlChangeHandler::OnNotify(PAUDIO_VOLUME_NOTIFICATION_DATA pNotify)
{
    FAST_FAIL(_callback->OnVolumeChanged(this->DeviceId.c_str(), pNotify->fMasterVolume));
    return S_OK;
}

HRESULT ControlChangeHandler::RegisterVolumeChangedCallback(IControlChangeCallback* callback)
{
    _callback = callback;
    return S_OK;
}