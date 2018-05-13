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
            internal const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
            internal const int KF_FLAG_DONT_VERIFY = 0x00004000;
            internal const int S_OK = 0;
            internal const int S_FALSE = 1;

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
        }

        public static bool IsImmersiveProcess(int processId)
        {
            var processHandle = Interop.OpenProcess(Interop.PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
            if (processHandle == IntPtr.Zero)
                return false;

            bool result = Interop.IsImmersiveProcess(processHandle) == Interop.S_OK;
            Interop.CloseHandle(processHandle);

            return result;
        }

        public static AppInformation GetInformationForAppByPid(int processId)
        {
            AppInformation info = new AppInformation();

            if (processId == 0)
            {
                return GetInformationForSystemProcess();
            }
            else
            {
                var processHandle = Interop.OpenProcess(Interop.PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
                if (processHandle != IntPtr.Zero)
                {
                    if (Interop.IsImmersiveProcess(processHandle) > 0)
                    {
                        int amuidBufferLength = Interop.MAX_AUMID_LEN;
                        var amuidBuffer = new StringBuilder(amuidBufferLength);

                        Interop.GetApplicationUserModelId(processHandle, ref amuidBufferLength, amuidBuffer);
                        Interop.CloseHandle(processHandle);

                        info.AppUserModelId = amuidBuffer.ToString();

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
                        var processFullPath = Process.GetProcessById(processId).GetMainModuleFileName();
                        info.SmallLogoPath = processFullPath;
                        info.DisplayName = Path.GetFileName(processFullPath);
                        info.AppUserModelId = processFullPath;
                        info.IsDesktopApp = true;
                    }
                }
            }

            return info;
        }

        private static AppInformation GetInformationForSystemProcess()
        {
            return new AppInformation()
            {
                AppUserModelId = "",
                BackgroundColor = 0x000000,
                DisplayName = "System Sounds",
                IsDesktopApp = true,
                PackageFullName = "",
                PackageInstallPath = "",
                SmallLogoPath = Environment.ExpandEnvironmentVariables(
                    $"%windir%\\{(Environment.Is64BitOperatingSystem ? "sysnative" : "system32")}\\audiosrv.dll")
            };
        }
    }
}
