#pragma once

namespace EarTrumpet
{
    namespace Interop
    {
        class AudioSessionService
        {
        private:
            static AudioSessionService* __instance;

            HRESULT GetAppProperties(PCWSTR pszAppId, PWSTR* ppszName, PWSTR* ppszIcon, ULONG *background);
            HRESULT GetAppUserModelIdFromPid(DWORD pid, LPWSTR* applicationUserModelId);
            HRESULT IsImmersiveProcess(DWORD pid);
			HRESULT CanResolveAppByApplicationUserModelId(LPCWSTR applicationUserModelId);

        public:
            static AudioSessionService* instance()
            {
                if (!__instance)
                {
                    __instance = new AudioSessionService;
                }
                return __instance;
            }

			HRESULT GetProcessProperties(DWORD processId, PWSTR* displayName, PWSTR* iconPath, BOOL* isDesktopApp, ULONG* backgroundColor);
        };
    }
}