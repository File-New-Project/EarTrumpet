#include "common.h"
#include <Audiopolicy.h>
#include "AudioSessionService.h"

using namespace EarTrumpet::Interop;

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