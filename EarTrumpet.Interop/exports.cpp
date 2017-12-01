#include "common.h"
#include <Audiopolicy.h>
#include <Mmdeviceapi.h>
#include <endpointvolume.h>

#include "AudioSessionService.h"

using namespace EarTrumpet::Interop;

// Sessions

extern "C" __declspec(dllexport) HRESULT GetProcessProperties(DWORD processId, LPWSTR* displayName, LPWSTR* iconPath, BOOL* isDesktopApp, ULONG* backgroundColor)
{
	return AudioSessionService::instance()->GetProcessProperties(processId, displayName, iconPath, isDesktopApp, backgroundColor);
}