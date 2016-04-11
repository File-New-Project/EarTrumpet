#include "common.h"
#include <Audiopolicy.h>
#include "AudioSessionService.h"
#include "AudioDeviceService.h"

using namespace EarTrumpet::Interop;

// Sessions

extern "C" __declspec(dllexport) HRESULT RefreshAudioSessions()
{
    return AudioSessionService::instance()->RefreshAudioSessions();
}

extern "C" __declspec(dllexport) int GetAudioSessionCount()
{
    return AudioSessionService::instance()->GetAudioSessionCount();
}

extern "C" __declspec(dllexport) HRESULT GetAudioSessions(void** audioSessions)
{
    return AudioSessionService::instance()->GetAudioSessions(audioSessions);
}

extern "C" __declspec(dllexport) HRESULT SetAudioSessionVolume(unsigned long sessionId, float volume)
{
    return AudioSessionService::instance()->SetAudioSessionVolume(sessionId, volume);
}

extern "C" __declspec(dllexport) HRESULT SetAudioSessionMute(unsigned long sessionId, bool isMuted)
{
    return AudioSessionService::instance()->SetAudioSessionMute(sessionId, isMuted);
}

// Devices

extern "C" __declspec(dllexport) HRESULT GetAudioDevices(void** audioDevices)
{
	return AudioDeviceService::instance()->GetAudioDevices(audioDevices);
}

extern "C" __declspec(dllexport) HRESULT SetDefaultAudioDevice(LPWSTR deviceId)
{
	return AudioDeviceService::instance()->SetDefaultAudioDevice(deviceId);
}

extern "C" __declspec(dllexport) HRESULT RefreshAudioDevices()
{
	return AudioDeviceService::instance()->RefreshAudioDevices();
}

extern "C" __declspec(dllexport) int GetAudioDeviceCount()
{
	return AudioDeviceService::instance()->GetAudioDeviceCount();
}
