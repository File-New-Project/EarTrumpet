#pragma once

namespace EarTrumpet
{
    namespace Interop
    {
        struct EarTrumpetAudioSession
        {
            wchar_t* DisplayName;
            wchar_t* IconPath;
            GUID GroupingId;
            unsigned long SessionId;
            unsigned long ProcessId;
            unsigned long BackgroundColor;
            float Volume;
            bool IsDesktopApp;
			bool IsMuted;
        };

        class AudioSessionService
        {
        private:
            static AudioSessionService* __instance;
                
            void CleanUpAudioSessions();
            HRESULT CreateEtAudioSessionFromAudioSession(CComPtr<IAudioSessionEnumerator> sessionEnumerator, int sessionCount, EarTrumpetAudioSession* etAudioSession);
            HRESULT GetAppProperties(PCWSTR pszAppId, PWSTR* ppszName, PWSTR* ppszIcon, ULONG *background);
            HRESULT GetAppUserModelIdFromPid(DWORD pid, LPWSTR* applicationUserModelId);
            HRESULT IsImmersiveProcess(DWORD pid);
			HRESULT CanResolveAppByApplicationUserModelId(LPCWSTR applicationUserModelId);

            std::vector<EarTrumpetAudioSession> _sessions;
            std::map<int, CComPtr<IAudioSessionControl2>> _sessionMap;
            
        public:
            static AudioSessionService* instance()
            {
                if (!__instance)
                {
                    __instance = new AudioSessionService;
                }
                return __instance;
            }

            int GetAudioSessionCount();
            HRESULT GetAudioSessions(void** audioSessions);
            HRESULT RefreshAudioSessions();
            HRESULT SetAudioSessionVolume(unsigned long sessionId, float volume);
			HRESULT SetAudioSessionMute(unsigned long sessionId, bool isMuted);
        };
    }
}