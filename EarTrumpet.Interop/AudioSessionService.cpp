#include "common.h"
#include <Audiopolicy.h>
#include <Mmdeviceapi.h>
#include <Appmodel.h>
#include <ShlObj.h>
#include <propkey.h>
#include <PathCch.h>
#include "AudioSessionService.h"
#include "ShellProperties.h"

using namespace std;
using namespace std::tr1;
using namespace EarTrumpet::Interop;

AudioSessionService* AudioSessionService::__instance = nullptr;

void AudioSessionService::CleanUpAudioSessions()
{
	for (auto session = _sessions.begin(); session != _sessions.end(); session++)
	{
		CoTaskMemFree(session->DisplayName);
		CoTaskMemFree(session->IconPath);
	}

	_sessions.clear();
	_sessionMap.clear();
}

int AudioSessionService::GetAudioSessionCount()
{
	return _sessions.size();
}

HRESULT AudioSessionService::RefreshAudioSessions()
{
	CleanUpAudioSessions();

	CComPtr<IMMDeviceEnumerator> deviceEnumerator;
	FAST_FAIL(CoCreateInstance(__uuidof(MMDeviceEnumerator), nullptr, CLSCTX_INPROC, IID_PPV_ARGS(&deviceEnumerator)));

	// TIP: Role parameter is not actually used https://msdn.microsoft.com/en-us/library/windows/desktop/dd371401.aspx
	CComPtr<IMMDevice> device;
	FAST_FAIL(deviceEnumerator->GetDefaultAudioEndpoint(EDataFlow::eRender, ERole::eMultimedia, &device));

	CComPtr<IAudioSessionManager2> audioSessionManager;
	FAST_FAIL(device->Activate(__uuidof(IAudioSessionManager2), CLSCTX_INPROC, nullptr, (void**)&audioSessionManager));

	CComPtr<IAudioSessionEnumerator> audioSessionEnumerator;
	FAST_FAIL(audioSessionManager->GetSessionEnumerator(&audioSessionEnumerator));

	int sessionCount;
	FAST_FAIL(audioSessionEnumerator->GetCount(&sessionCount));

	for (int i = 0; i < sessionCount; i++)
	{
		EarTrumpetAudioSession audioSession;
		if (SUCCEEDED(CreateEtAudioSessionFromAudioSession(audioSessionEnumerator, i, &audioSession)))
		{
			_sessions.push_back(audioSession);
		}
	}

	return S_OK;
}

HRESULT AudioSessionService::CreateEtAudioSessionFromAudioSession(CComPtr<IAudioSessionEnumerator> audioSessionEnumerator, int sessionCount, EarTrumpetAudioSession* etAudioSession)
{
	CComPtr<IAudioSessionControl> audioSessionControl;
	FAST_FAIL(audioSessionEnumerator->GetSession(sessionCount, &audioSessionControl));

	CComPtr<IAudioSessionControl2> audioSessionControl2;
	FAST_FAIL(audioSessionControl->QueryInterface(IID_PPV_ARGS(&audioSessionControl2)));

	DWORD pid;
	FAST_FAIL(audioSessionControl2->GetProcessId(&pid));

    etAudioSession->ProcessId = pid;

    FAST_FAIL(audioSessionControl2->GetGroupingParam(&etAudioSession->GroupingId));

	CComHeapPtr<wchar_t> sessionIdString;
	FAST_FAIL(audioSessionControl2->GetSessionIdentifier(&sessionIdString));

	hash<wstring> stringHash;
	etAudioSession->SessionId = stringHash(static_cast<PWSTR>(sessionIdString));

	_sessionMap[etAudioSession->SessionId] = audioSessionControl2;

	CComPtr<ISimpleAudioVolume> simpleAudioVolume;
	FAST_FAIL(audioSessionControl->QueryInterface(IID_PPV_ARGS(&simpleAudioVolume)));
	FAST_FAIL(simpleAudioVolume->GetMasterVolume(&etAudioSession->Volume));

	if (IsImmersiveProcess(pid))
	{
		PWSTR appUserModelId;
		FAST_FAIL(GetAppUserModelIdFromPid(pid, &appUserModelId));

		FAST_FAIL(GetAppProperties(appUserModelId, &etAudioSession->DisplayName, &etAudioSession->IconPath, &etAudioSession->BackgroundColor));

		etAudioSession->IsDesktopApp = false;
	}
	else
	{
        bool isSystemSoundsSession = (S_OK == audioSessionControl2->IsSystemSoundsSession());

		AudioSessionState state;
		FAST_FAIL(audioSessionControl2->GetState(&state));
		if (!isSystemSoundsSession && (state == AudioSessionState::AudioSessionStateExpired))
		{
			return E_NOT_VALID_STATE;
		}
		
		if (isSystemSoundsSession)
		{
            PCWSTR pszDllPath;
            BOOL isWow64Process;
            if (!IsWow64Process(GetCurrentProcess(), &isWow64Process) || isWow64Process)
            {
                pszDllPath = L"%windir%\\sysnative\\audiosrv.dll";
            }
            else
            {
                pszDllPath = L"%windir%\\system32\\audiosrv.dll";
            }

            wchar_t szPath[MAX_PATH] = {};
            if (0 == ExpandEnvironmentStrings(pszDllPath, szPath, ARRAYSIZE(szPath)))
            {
                return E_FAIL;
            }

            FAST_FAIL(SHStrDup(pszDllPath, &etAudioSession->IconPath));
            FAST_FAIL(SHStrDup(L"System Sounds", &etAudioSession->DisplayName));
		}
		else
		{
			shared_ptr<void> processHandle(OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, pid), CloseHandle);
			FAST_FAIL_HANDLE(processHandle.get());

			wchar_t imagePath[MAX_PATH] = {};
			DWORD dwCch = ARRAYSIZE(imagePath);
            FAST_FAIL(QueryFullProcessImageName(processHandle.get(), 0, imagePath, &dwCch) == 0 ? E_FAIL : S_OK);

            FAST_FAIL(SHStrDup(imagePath, &etAudioSession->IconPath));
            FAST_FAIL(SHStrDup(PathFindFileName(imagePath), &etAudioSession->DisplayName));
		}

		etAudioSession->IsDesktopApp = true;
		etAudioSession->BackgroundColor = 0x00000000;
	}

	return S_OK;
}

HRESULT AudioSessionService::GetAudioSessions(void** audioSessions)
{
	if (_sessions.size() == 0)
	{
		return HRESULT_FROM_WIN32(ERROR_NO_MORE_ITEMS);
	}

	*audioSessions = &_sessions[0];
	return S_OK;
}

BOOL AudioSessionService::IsImmersiveProcess(DWORD pid)
{
	shared_ptr<void> processHandle(OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, pid), CloseHandle);
	FAST_FAIL_HANDLE(processHandle.get());

	return ::IsImmersiveProcess(processHandle.get());
}

HRESULT AudioSessionService::GetAppUserModelIdFromPid(DWORD pid, LPWSTR* applicationUserModelIdPtr)
{
	shared_ptr<void> processHandle(OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, pid), CloseHandle);
	FAST_FAIL_HANDLE(processHandle.get());

	unsigned int appUserModelIdLength = 0;
	long returnCode = GetApplicationUserModelId(processHandle.get(), &appUserModelIdLength, NULL);
	if (returnCode != ERROR_INSUFFICIENT_BUFFER)
	{
		return HRESULT_FROM_WIN32(returnCode);
	}

	unique_ptr<wchar_t[]> appUserModelId(new wchar_t[appUserModelIdLength]);
	returnCode = GetApplicationUserModelId(processHandle.get(), &appUserModelIdLength, appUserModelId.get());
	if (returnCode != ERROR_SUCCESS)
	{
		return HRESULT_FROM_WIN32(returnCode);
	}

	FAST_FAIL(SHStrDup(appUserModelId.get(), applicationUserModelIdPtr));

	return S_OK;
}

HRESULT AudioSessionService::SetAudioSessionVolume(unsigned long sessionId, float volume)
{
	if (!_sessionMap[sessionId])
	{
		return E_INVALIDARG;
	}

	CComPtr<ISimpleAudioVolume> simpleAudioVolume;
	FAST_FAIL(_sessionMap[sessionId]->QueryInterface(IID_PPV_ARGS(&simpleAudioVolume)));

	FAST_FAIL(simpleAudioVolume->SetMasterVolume(volume, nullptr));
	
	return S_OK;
}

HRESULT AudioSessionService::GetAppProperties(PCWSTR pszAppId, PWSTR* ppszName, PWSTR* ppszIcon, ULONG *background)
{
	*ppszIcon = nullptr;
	*ppszName = nullptr;
	*background = 0;

	CComPtr<IShellItem2> item;
	FAST_FAIL(SHCreateItemInKnownFolder(FOLDERID_AppsFolder, KF_FLAG_DONT_VERIFY, pszAppId, IID_PPV_ARGS(&item)));

	CComHeapPtr<wchar_t> itemName;
	FAST_FAIL(item->GetString(PKEY_ItemNameDisplay, &itemName));
	FAST_FAIL(item->GetUInt32(PKEY_AppUserModel_Background, background));

	CComHeapPtr<wchar_t> installPath;
	FAST_FAIL(item->GetString(PKEY_AppUserModel_PackageInstallPath, &installPath));

	CComHeapPtr<wchar_t> iconPath;
	FAST_FAIL(item->GetString(PKEY_AppUserModel_Icon, &iconPath));

	wchar_t fullPath[MAX_PATH] = {};
	FAST_FAIL(PathCchCombine(fullPath, ARRAYSIZE(fullPath), installPath, iconPath));

	CStringW path(fullPath);

	if (!PathFileExists(path))
	{
		path.Replace(L".png", L".scale-100.png");
	}

	FAST_FAIL(SHStrDup(path, ppszIcon));
	*ppszName = itemName.Detach();
	return S_OK;
}