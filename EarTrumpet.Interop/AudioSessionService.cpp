#include "common.h"
#include <Audiopolicy.h>
#include <Mmdeviceapi.h>
#include <Appmodel.h>
#include <ShlObj.h>
#include <Shlwapi.h>
#include <propkey.h>
#include <PathCch.h>
#include "AudioSessionService.h"
#include "ShellProperties.h"
#include "MrtResourceManager.h"

using namespace std;
using namespace std::tr1;
using namespace EarTrumpet::Interop;

AudioSessionService* AudioSessionService::__instance = nullptr;

struct PackageInfoReferenceDeleter
{
    void operator()(PACKAGE_INFO_REFERENCE* reference)
    {
        ClosePackageInfo(*reference);
    }
};

typedef unique_ptr<PACKAGE_INFO_REFERENCE, PackageInfoReferenceDeleter> PackageInfoReference;

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
    FAST_FAIL(audioSessionControl2->GetSessionInstanceIdentifier(&sessionIdString));

    hash<wstring> stringHash;
    etAudioSession->SessionId = stringHash(static_cast<PWSTR>(sessionIdString));

    _sessionMap[etAudioSession->SessionId] = audioSessionControl2;

    CComPtr<ISimpleAudioVolume> simpleAudioVolume;
    FAST_FAIL(audioSessionControl->QueryInterface(IID_PPV_ARGS(&simpleAudioVolume)));
    FAST_FAIL(simpleAudioVolume->GetMasterVolume(&etAudioSession->Volume));

    BOOL isMuted;
    FAST_FAIL(simpleAudioVolume->GetMute(&isMuted));
    etAudioSession->IsMuted = !!isMuted;

    HRESULT hr = IsImmersiveProcess(pid);
    if (hr == S_OK)
    {
        PWSTR appUserModelId;
        FAST_FAIL(GetAppUserModelIdFromPid(pid, &appUserModelId));

        FAST_FAIL(GetAppProperties(appUserModelId, &etAudioSession->DisplayName, &etAudioSession->IconPath, &etAudioSession->BackgroundColor));

        etAudioSession->IsDesktopApp = false;
    }
    else if (hr == S_FALSE)
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

HRESULT AudioSessionService::IsImmersiveProcess(DWORD pid)
{
    shared_ptr<void> processHandle(OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, pid), CloseHandle);
    FAST_FAIL_HANDLE(processHandle.get());
    return (::IsImmersiveProcess(processHandle.get()) ? S_OK : S_FALSE);
}

HRESULT AudioSessionService::CanResolveAppByApplicationUserModelId(LPCWSTR applicationUserModelId)
{
    CComPtr<IShellItem2> item;
    return SUCCEEDED(SHCreateItemInKnownFolder(FOLDERID_AppsFolder, KF_FLAG_DONT_VERIFY, applicationUserModelId, IID_PPV_ARGS(&item)));
}

HRESULT AudioSessionService::GetAppUserModelIdFromPid(DWORD pid, LPWSTR* applicationUserModelId)
{
    shared_ptr<void> processHandle(OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, pid), CloseHandle);
    FAST_FAIL_HANDLE(processHandle.get());

    unsigned int appUserModelIdLength = 0;
    long returnCode = GetApplicationUserModelId(processHandle.get(), &appUserModelIdLength, nullptr);
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

    if (CanResolveAppByApplicationUserModelId(appUserModelId.get()))
    {
        FAST_FAIL(SHStrDup(appUserModelId.get(), applicationUserModelId));
    }
    else
    {
        wchar_t packageFamilyName[PACKAGE_FAMILY_NAME_MAX_LENGTH];
        UINT32 packageFamilyNameLength = ARRAYSIZE(packageFamilyName);
        wchar_t packageRelativeAppId[PACKAGE_RELATIVE_APPLICATION_ID_MAX_LENGTH];
        UINT32 packageRelativeAppIdLength = ARRAYSIZE(packageRelativeAppId);

        FAST_FAIL_WIN32(ParseApplicationUserModelId(appUserModelId.get(), &packageFamilyNameLength, packageFamilyName, &packageRelativeAppIdLength, packageRelativeAppId));

        UINT32 packageCount = 0;
        UINT32 packageNamesBufferLength = 0;
        FAST_FAIL_BUFFER(FindPackagesByPackageFamily(packageFamilyName, PACKAGE_FILTER_HEAD | PACKAGE_INFORMATION_BASIC, &packageCount, nullptr, &packageNamesBufferLength, nullptr, nullptr));

        if (packageCount <= 0)
        {
            return E_NOTFOUND;
        }

        unique_ptr<PWSTR[]> packageNames(new PWSTR[packageCount]);
        unique_ptr<wchar_t[]> buffer(new wchar_t[packageNamesBufferLength]);
        FAST_FAIL_WIN32(FindPackagesByPackageFamily(packageFamilyName, PACKAGE_FILTER_HEAD | PACKAGE_INFORMATION_BASIC, &packageCount, packageNames.get(), &packageNamesBufferLength, buffer.get(), nullptr));

        PackageInfoReference packageInfoRef;
        PACKAGE_INFO_REFERENCE rawPackageInfoRef;
        FAST_FAIL_WIN32(OpenPackageInfoByFullName(packageNames[0], 0, &rawPackageInfoRef));
        packageInfoRef.reset(&rawPackageInfoRef);

        UINT32 packageIdsLength = 0;
        UINT32 packageIdCount = 0;
        FAST_FAIL_BUFFER(GetPackageApplicationIds(*packageInfoRef.get(), &packageIdsLength, nullptr, &packageIdCount));

        if (packageIdCount <= 0)
        {
            return E_NOTFOUND;
        }

        unique_ptr<BYTE[]> packageIdsRaw(new BYTE[packageIdsLength]);
        FAST_FAIL_WIN32(GetPackageApplicationIds(*packageInfoRef.get(), &packageIdsLength, packageIdsRaw.get(), &packageIdCount));

        PCWSTR* packageIds = reinterpret_cast<PCWSTR*>(packageIdsRaw.get());
        FAST_FAIL(SHStrDup(packageIds[0], applicationUserModelId));
    }
    
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

HRESULT AudioSessionService::SetAudioSessionMute(unsigned long sessionId, bool isMuted)
{
    if (!_sessionMap[sessionId])
    {
        return E_INVALIDARG;
    }

    CComPtr<ISimpleAudioVolume> simpleAudioVolume;
    FAST_FAIL(_sessionMap[sessionId]->QueryInterface(IID_PPV_ARGS(&simpleAudioVolume)));

    FAST_FAIL(simpleAudioVolume->SetMute(isMuted, nullptr));
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

    LPWSTR resolvedIconPath;
    if (UrlIsFileUrl(iconPath))
    {
        FAST_FAIL(PathCreateFromUrlAlloc(iconPath, &resolvedIconPath, 0));
    }
    else
    {
        CComHeapPtr<wchar_t> fullPackagePath;
        FAST_FAIL(item->GetString(PKEY_AppUserModel_PackageFullName, &fullPackagePath));

        CComPtr<IMrtResourceManager> mrtResMgr;
        FAST_FAIL(CoCreateInstance(__uuidof(MrtResourceManager), nullptr, CLSCTX_INPROC, IID_PPV_ARGS(&mrtResMgr)));
        FAST_FAIL(mrtResMgr->InitializeForPackage(fullPackagePath));

        CComPtr<IResourceMap> resourceMap;
        FAST_FAIL(mrtResMgr->GetMainResourceMap(IID_PPV_ARGS(&resourceMap)));
        FAST_FAIL(resourceMap->GetFilePath(iconPath, &resolvedIconPath));
    }

    *ppszIcon = resolvedIconPath;
    *ppszName = itemName.Detach();

    return S_OK;
}