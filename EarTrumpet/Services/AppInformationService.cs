using EarTrumpet.DataModel;
using EarTrumpet.DataModel.Com;
using EarTrumpet.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace EarTrumpet.Services
{
    public class AppInformationService
    {
        internal static class Interop
        {
            internal const int MAX_AUMID_LEN = 512;
            internal const int PACKAGE_FAMILY_NAME_MAX_LENGTH_INCL_Z = 65 * 2;
            internal const int PACKAGE_RELATIVE_APPLICATION_ID_MAX_LENGTH_INCL_Z = 65 * 2;
            internal const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
            internal const int KF_FLAG_DONT_VERIFY = 0x00004000;
            internal const int PACKAGE_INFORMATION_BASIC = 0x00000000;
            internal const int PACKAGE_FILTER_HEAD = 0x00000010;

            [DllImport("kernel32.dll", PreserveSig = true)]
            internal static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

            [DllImport("kernel32.dll", PreserveSig = true)]
            internal static extern bool CloseHandle(IntPtr hObject);

            [DllImport("user32.dll", PreserveSig = true)]
            internal static extern int IsImmersiveProcess(IntPtr hProcess);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
            internal static extern void GetApplicationUserModelId(
                IntPtr hProcess,
                ref int applicationUserModelIdLength,
                [MarshalAs(UnmanagedType.LPWStr)]StringBuilder applicationUserModelId);

            [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
            internal static extern void SHCreateItemInKnownFolder(
                ref Guid kfid,
                uint dwKFFlags,
                [MarshalAs(UnmanagedType.LPWStr)]string pszItem,
                ref Guid riid,
                [MarshalAs(UnmanagedType.Interface)]out IShellItem2 ppv);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
            internal static extern int ParseApplicationUserModelId(
                [MarshalAs(UnmanagedType.LPWStr)]string applicationUserModelId,
                ref int packageFamilyNameLength,
                StringBuilder packageFamilyName,
                ref int packageRelativeApplicationIdLength,
                StringBuilder packageRelativeApplicationId);

            [DllImport("kernel32.dll", EntryPoint = "FindPackagesByPackageFamily", CharSet = CharSet.Unicode, PreserveSig = true)]
            internal static extern int FindPackagesByPackageFamilyInitial(
                [MarshalAs(UnmanagedType.LPWStr)]string packageFamilyName,
                int packageFilters,
                ref int count,
                IntPtr packageFullNames,
                ref int bufferLength,
                IntPtr buffer,
                IntPtr packageProperties);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
            internal static extern int FindPackagesByPackageFamily(
                [MarshalAs(UnmanagedType.LPWStr)]string packageFamilyName,
                int packageFilters,
                ref int count,
                IntPtr[] packageFullNames,
                ref int bufferLength,
                IntPtr buffer,
                IntPtr packageProperties);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
            internal static extern int OpenPackageInfoByFullName(
                [MarshalAs(UnmanagedType.LPWStr)]string packageFullName,
                int reserved,
                out IntPtr packageInfoReference);

            [DllImport("kernel32.dll", PreserveSig = true)]
            internal static extern int GetPackageApplicationIds(
                IntPtr packageInfoReference,
                ref int bufferLength,
                IntPtr buffer,
                out int count);

            [DllImport("kernel32.dll", PreserveSig = true)]
            internal static extern int ClosePackageInfo(
                IntPtr packageInfoReference);
        }

        public static bool IsImmersiveProcess(int processId)
        {
            var processHandle = Interop.OpenProcess(Interop.PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
            if (processHandle == IntPtr.Zero)
                return false;

            bool result = Interop.IsImmersiveProcess(processHandle) != 0;
            Interop.CloseHandle(processHandle);
            return result;
        }

        private static bool CanResolveAppByApplicationUserModelId(string aumid)
        {
            try
            {
                Interop.SHCreateItemInKnownFolder(
                ref FolderIds.AppsFolder,
                Interop.KF_FLAG_DONT_VERIFY,
                aumid,
                ref IIDs.IID_IShellItem2,
                out IShellItem2 shellitem);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetAppUserModelIdByPid(int processId)
        {
            string appUserModelId = string.Empty;

            var processHandle = Interop.OpenProcess(Interop.PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
            if (processHandle != IntPtr.Zero)
            {
                int amuidBufferLength = Interop.MAX_AUMID_LEN;
                var amuidBuffer = new StringBuilder(amuidBufferLength);

                Interop.GetApplicationUserModelId(processHandle, ref amuidBufferLength, amuidBuffer);
                appUserModelId = amuidBuffer.ToString();
                Interop.CloseHandle(processHandle);

                //
                // We may receive an AUMID for an app in a package that doesn't have
                // the metadata we need (e.g. Skype). If the AUMID doesn't resolve to
                // an app, we need to bust open the package, find the main app, and
                // retrieve the metadata we need.
                //

                if (!CanResolveAppByApplicationUserModelId(appUserModelId))
                {
                    int packageFamilyNameLength = Interop.PACKAGE_FAMILY_NAME_MAX_LENGTH_INCL_Z;
                    var packageFamilyNameBuilder = new StringBuilder(packageFamilyNameLength);

                    int packageRelativeApplicationIdLength = Interop.PACKAGE_RELATIVE_APPLICATION_ID_MAX_LENGTH_INCL_Z;
                    var packageRelativeApplicationIdBuilder = new StringBuilder(packageRelativeApplicationIdLength);

                    Interop.ParseApplicationUserModelId(
                        appUserModelId,
                        ref packageFamilyNameLength,
                        packageFamilyNameBuilder,
                        ref packageRelativeApplicationIdLength,
                        packageRelativeApplicationIdBuilder);

                    string packageFamilyName = packageFamilyNameBuilder.ToString();

                    int packageCount = 0;
                    int packageNamesBufferLength = 0;
                    Interop.FindPackagesByPackageFamilyInitial(
                        packageFamilyName,
                        Interop.PACKAGE_FILTER_HEAD | Interop.PACKAGE_INFORMATION_BASIC,
                        ref packageCount,
                        IntPtr.Zero,
                        ref packageNamesBufferLength,
                        IntPtr.Zero,
                        IntPtr.Zero);

                    if (packageCount > 0)
                    {
                        var pointers = new IntPtr[packageCount];
                        IntPtr buffer = Marshal.AllocHGlobal(packageNamesBufferLength * 2 /* sizeof(wchar_t) */);

                        Interop.FindPackagesByPackageFamily(
                            packageFamilyName,
                            Interop.PACKAGE_FILTER_HEAD | Interop.PACKAGE_INFORMATION_BASIC,
                            ref packageCount,
                            pointers,
                            ref packageNamesBufferLength,
                            buffer,
                            IntPtr.Zero);

                        var packageFullName = Marshal.PtrToStringUni(pointers[0]);
                        Marshal.FreeHGlobal(buffer);

                        Interop.OpenPackageInfoByFullName(packageFullName, 0, out IntPtr packageInfoReference);

                        int bufferLength = 0;
                        Interop.GetPackageApplicationIds(packageInfoReference, ref bufferLength, IntPtr.Zero, out int appIdCount);

                        buffer = Marshal.AllocHGlobal(bufferLength);
                        Interop.GetPackageApplicationIds(packageInfoReference, ref bufferLength, buffer, out appIdCount);

                        appUserModelId = Marshal.PtrToStringUni(Marshal.ReadIntPtr(buffer));
                        Marshal.FreeHGlobal(buffer);

                        Interop.ClosePackageInfo(packageInfoReference);
                    }
                }
            }

            return appUserModelId;
        }

        public static AppInformation GetInformationForAppByPid(int processId)
        {
            AppInformation info = new AppInformation();

            if (processId == 0)
            {
                return GetInformationForSystemProcess();
            }

            if (IsImmersiveProcess(processId))
            {
                info.AppUserModelId = GetAppUserModelIdByPid(processId);

                Interop.SHCreateItemInKnownFolder(
                    ref FolderIds.AppsFolder,
                    Interop.KF_FLAG_DONT_VERIFY,
                    info.AppUserModelId,
                    ref IIDs.IID_IShellItem2,
                    out IShellItem2 shellitem);

                info.DisplayName = shellitem.GetString(ref PropertyKeys.PKEY_ItemNameDisplay);
                info.BackgroundColor = shellitem.GetUInt32(ref PropertyKeys.PKEY_AppUserModel_Background);
                info.PackageInstallPath = shellitem.GetString(ref PropertyKeys.PKEY_AppUserModel_PackageInstallPath);
                info.PackageFullName = shellitem.GetString(ref PropertyKeys.PKEY_AppUserModel_PackageFullName);

                string rawSmallLogoPath = shellitem.GetString(ref PropertyKeys.PKEY_Tile_SmallLogoPath);
                if (Uri.IsWellFormedUriString(rawSmallLogoPath, UriKind.RelativeOrAbsolute))
                {
                    info.SmallLogoPath = new Uri(rawSmallLogoPath).LocalPath;
                }
                else
                {
                    var mrtResourceManager = (IMrtResourceManager)new MrtResourceManager();
                    mrtResourceManager.InitializeForPackage(info.PackageFullName);
                    mrtResourceManager.GetMainResourceMap(ref IIDs.IID_ResourceMap, out IResourceMap map);

                    map.GetFilePath(rawSmallLogoPath, out string mrtSmallLogoPath);
                    info.SmallLogoPath = Path.Combine(info.PackageInstallPath, mrtSmallLogoPath);
                }
            }
            else
            {
                try
                {
                    var processFullPath = Process.GetProcessById(processId).GetMainModuleFileName();
                    info.DisplayName = Path.GetFileName(processFullPath);
                    info.SmallLogoPath = processFullPath;
                    info.AppUserModelId = processFullPath;
                    info.IsDesktopApp = true;
                }
                catch (System.ArgumentException)
                {
                    /* Process disappeared */
                }
            }

            return info;
        }

        private static AppInformation GetInformationForSystemProcess()
        {
            return new AppInformation()
            {
                BackgroundColor = 0x000000,
                IsDesktopApp = true,
                SmallLogoPath = Environment.ExpandEnvironmentVariables(
                    $"%windir%\\{(Environment.Is64BitOperatingSystem ? "sysnative" : "system32")}\\audiosrv.dll")
            };
        }
    }
}
