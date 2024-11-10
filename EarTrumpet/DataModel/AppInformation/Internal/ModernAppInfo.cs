using EarTrumpet.Interop;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.UI.Shell;
using Windows.Win32.System.Threading;
using Windows.Win32.Storage.Packaging.Appx;

namespace EarTrumpet.DataModel.AppInformation.Internal;

internal class ModernAppInfo : IAppInfo
{
    public event Action<IAppInfo> Stopped;

    public string AppId { get; }
    public string ExeName { get; }
    public string DisplayName { get; }
    public string PackageInstallPath { get; }
    public string SmallLogoPath { get; }
    public bool IsDesktopApp => false;

    public ModernAppInfo(uint processId, bool trackProcess)
    {
        var appUserModelId = GetAppUserModelIdByPid(processId);

        try
        {
            PInvoke.SHCreateItemInKnownFolder(
                PInvoke.FOLDERID_AppsFolder,
                KNOWN_FOLDER_FLAG.KF_FLAG_DONT_VERIFY,
                appUserModelId,
                typeof(IShellItem2).GUID,
                out var rawShellItem);

            var shellItem = (IShellItem2)rawShellItem;

            var pkey = PInvoke.PKEY_AppUserModel_PackageInstallPath;
            var packageInstallPathPtr = new PWSTR();
            unsafe
            {
                shellItem.GetString(&pkey, &packageInstallPathPtr);
            }

            pkey = PInvoke.PKEY_ItemNameDisplay;
            var displayNamePtr = new PWSTR();
            unsafe
            {
                shellItem.GetString(&pkey, &displayNamePtr);
            }

            pkey = PInvoke.PKEY_AppUserModel_PackageInstallPath;
            var aumidPtr = new PWSTR();
            unsafe
            {
                shellItem.GetString(&pkey, &aumidPtr);
            }

            AppId = aumidPtr.ToString();
            PackageInstallPath = packageInstallPathPtr.ToString();
            DisplayName = displayNamePtr.ToString();
            ExeName = packageInstallPathPtr.ToString();
            SmallLogoPath = appUserModelId;
        }
        catch (COMException ex)
        {
            Trace.WriteLine($"ModernAppInfo AppsFolder lookup failed 0x{((uint)ex.HResult).ToString("x", CultureInfo.CurrentCulture)} {appUserModelId}");
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"ModernAppInfo AppsFolder lookup failed {appUserModelId} {ex}");
        }

        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            DisplayName = appUserModelId;
        }

        if (trackProcess)
        {
            ProcessWatcherService.WatchProcess(processId, (pid) => Stopped?.Invoke(this));
        }
    }

    private static string GetAppUserModelIdByPid(uint processId)
    {
        var appUserModelId = string.Empty;

        var processHandle = PInvoke.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION | PROCESS_ACCESS_RIGHTS.PROCESS_SYNCHRONIZE, false, processId);
        if (processHandle != IntPtr.Zero)
        {
            try
            {
                ZombieProcessException.ThrowIfZombie(processId, processHandle);

                var amuidBufferLength = Kernel32.MAX_AUMID_LEN;
                Span<char> amuidBuffer = stackalloc char[(int)amuidBufferLength];
                unsafe
                {
                    fixed (char* pAmuidBuffer = amuidBuffer)
                    {
                        if (PInvoke.GetApplicationUserModelId(processHandle, &amuidBufferLength, pAmuidBuffer) == WIN32_ERROR.NO_ERROR)
                        {
                            appUserModelId = new PWSTR(pAmuidBuffer).ToString();
                        }
                    }
                }
            }
            finally
            {
                PInvoke.CloseHandle(processHandle);
            }

            // We may receive an AUMID for an app in a package that doesn't have
            // the metadata we need (e.g. Skype). If the AUMID doesn't resolve to
            // an app, we need to bust open the package, find the main app, and
            // retrieve the metadata we need.
            if (!CanResolveAppByApplicationUserModelId(appUserModelId))
            {
                Span<char> packageFamilyName = stackalloc char[Kernel32.PACKAGE_FAMILY_NAME_MAX_LENGTH_INCL_Z];
                Span<char> packageRelativeApplicationId = stackalloc char[Kernel32.PACKAGE_FAMILY_NAME_MAX_LENGTH_INCL_Z];
                var packageFamilyNameLength = (uint)packageFamilyName.Length;
                var packageRelativeApplicationIdLength = (uint)packageRelativeApplicationId.Length;

                unsafe
                {
                    fixed (char* packageFamilyNamePtr = packageFamilyName)
                    fixed (char* packageNamePtr = packageRelativeApplicationId)
                    {
                        _ = PInvoke.ParseApplicationUserModelId(
                            appUserModelId,
                            ref packageFamilyNameLength,
                            packageFamilyNamePtr,
                            ref packageRelativeApplicationIdLength,
                            packageNamePtr);
                    }
                }

                var packageCount = 0U;
                var packageNamesBufferLength = 0U;
                unsafe
                {
                    fixed (char* packageFamilyNamePtr = packageFamilyName)
                    {
                        _ = PInvoke.FindPackagesByPackageFamily(
                        new PWSTR(packageFamilyNamePtr).ToString(),
                        PInvoke.PACKAGE_FILTER_HEAD | PInvoke.PACKAGE_INFORMATION_BASIC,
                        ref packageCount,
                        null,
                        ref packageNamesBufferLength,
                        null,
                        null);
                    }
                }

                if (packageCount > 0)
                {
                    var stringPointers = new PWSTR[packageCount];
                    unsafe
                    {
                        Span<char> stringBuffer = stackalloc char[(int)(packageNamesBufferLength * Kernel32.SIZEOF_WCHAR)];
                        fixed (char* ptrStringBuffer = stringBuffer)
                        {
                            fixed (char* packageFamilyNamePtr = packageFamilyName)
                            fixed (PWSTR* ptrStringPointers = stringPointers)
                            {
                                _ = PInvoke.FindPackagesByPackageFamily(
                                    new PWSTR(packageFamilyNamePtr).ToString(),
                                    PInvoke.PACKAGE_FILTER_HEAD | PInvoke.PACKAGE_INFORMATION_BASIC,
                                    ref packageCount,
                                    ptrStringPointers,
                                    ref packageNamesBufferLength,
                                    new PWSTR(ptrStringBuffer),
                                    null);
                            }
                        }
                    }

                    var packageFullName = stringPointers[0].ToString();

                    unsafe
                    {
                        _ = PInvoke.OpenPackageInfoByFullName(packageFullName, out var packageInfoReference);

                        var bufferLength = 0U;
                        _ = PInvoke.GetPackageApplicationIds(packageInfoReference, &bufferLength);

                        Span<char> stringBuffer = stackalloc char[(int)bufferLength];
                        fixed (char* ptrStringBuffer = stringBuffer)
                        {
                            _ = PInvoke.GetPackageApplicationIds(packageInfoReference, &bufferLength, (byte*)ptrStringBuffer);
                            appUserModelId = new PWSTR(ptrStringBuffer).ToString();
                        }
                        
                        _ = PInvoke.ClosePackageInfo(packageInfoReference);
                    }
                }
            }
        }
        else
        {
            throw new ZombieProcessException(processId);
        }

        return appUserModelId;
    }

    private static bool CanResolveAppByApplicationUserModelId(string aumid)
    {
        try
        {
            PInvoke.SHCreateItemInKnownFolder(
                PInvoke.FOLDERID_AppsFolder,
                KNOWN_FOLDER_FLAG.KF_FLAG_DONT_VERIFY,
                aumid,
                typeof(IShellItem2).GUID,
                out var rawShellItem).ThrowOnFailure();
            return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"{ex}");
            return false;
        }
    }
}
