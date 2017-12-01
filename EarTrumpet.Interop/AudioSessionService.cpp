#include "common.h"
#include <Audiopolicy.h>
#include <Endpointvolume.h>
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

HRESULT AudioSessionService::IsImmersiveProcess(DWORD pid)
{
    shared_ptr<void> processHandle(OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, pid), CloseHandle);
    FAST_FAIL_HANDLE(processHandle.get());
    return (::IsImmersiveProcess(processHandle.get()) ? S_OK : S_FALSE);
}

HRESULT AudioSessionService::CanResolveAppByApplicationUserModelId(LPCWSTR applicationUserModelId)
{
    CComPtr<IShellItem2> item;
    return SHCreateItemInKnownFolder(FOLDERID_AppsFolder, KF_FLAG_DONT_VERIFY, applicationUserModelId, IID_PPV_ARGS(&item));
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

    if (SUCCEEDED(CanResolveAppByApplicationUserModelId(appUserModelId.get())))
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
    FAST_FAIL(item->GetString(PKEY_Tile_SmallLogoPath, &iconPath));

    LPWSTR resolvedIconPath;
    if (UrlIsFileUrl(iconPath))
    {
        FAST_FAIL(PathCreateFromUrlAlloc(iconPath, &resolvedIconPath, 0));
    }
    else
    {
        CComHeapPtr<wchar_t> fullPackageName;
        FAST_FAIL(item->GetString(PKEY_AppUserModel_PackageFullName, &fullPackageName));

        CComPtr<IMrtResourceManager> mrtResMgr;
        FAST_FAIL(CoCreateInstance(__uuidof(MrtResourceManager), nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&mrtResMgr)));
        FAST_FAIL(mrtResMgr->InitializeForPackage(fullPackageName));

        CComPtr<IResourceMap> resourceMap;
        FAST_FAIL(mrtResMgr->GetMainResourceMap(IID_PPV_ARGS(&resourceMap)));
		if (FAILED(resourceMap->GetFilePath(iconPath, &resolvedIconPath)))
		{
			FAST_FAIL(PathAllocCombine(installPath, iconPath, 0, &resolvedIconPath));
		}
    }

    *ppszIcon = resolvedIconPath;
    *ppszName = itemName.Detach();
    return S_OK;
}

HRESULT AudioSessionService::GetProcessProperties(DWORD processId, PWSTR* displayName, PWSTR* iconPath, BOOL* isDesktopApp, ULONG* backgroundColor)
{
	HRESULT hr = IsImmersiveProcess(processId);
	if (hr == S_OK)
	{
		PWSTR appUserModelId;
		FAST_FAIL(GetAppUserModelIdFromPid(processId, &appUserModelId));

		FAST_FAIL(GetAppProperties(appUserModelId, displayName, iconPath, backgroundColor));

		*isDesktopApp = FALSE;
	}
	else if (hr == S_FALSE)
	{
		shared_ptr<void> processHandle(OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, processId), CloseHandle);
		FAST_FAIL_HANDLE(processHandle.get());

		wchar_t imagePath[MAX_PATH] = {};
		DWORD dwCch = ARRAYSIZE(imagePath);
		FAST_FAIL(QueryFullProcessImageName(processHandle.get(), 0, imagePath, &dwCch) == 0 ? E_FAIL : S_OK);
		FAST_FAIL(SHStrDup(imagePath, iconPath));
		FAST_FAIL(SHStrDup(PathFindFileName(imagePath), displayName));

		*isDesktopApp = TRUE;
		*backgroundColor = 0x00000000;
	}
	return S_OK;
}