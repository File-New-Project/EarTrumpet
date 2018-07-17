using EarTrumpet.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace EarTrumpet.DataModel.Internal.Services
{
    public class ZombieProcessException : Exception
    {
        public ZombieProcessException(int processId) : base($"Process is a zombie: {processId}") { }
    }

    class AppInformationService
    {
        private static IShellItem2 GetShellItemForAppByAumid(string aumid)
        {
            var iid = typeof(IShellItem2).GUID;
            return Shell32.SHCreateItemInKnownFolder(ref FolderIds.AppsFolder, Shell32.KF_FLAG_DONT_VERIFY, aumid, ref iid);
        }

        internal static AppInformation GetInformationForAppByPid(int processId)
        {
            Trace.WriteLine($"AppInformationService GetInformationForAppByPid {processId}");

            var appInfo = new AppInformation();
            appInfo.CanTrack = true;

            if (processId == 0)
            {
                return GetInformationForSystemProcess();
            }

            if (IsImmersiveProcess(processId))
            {
                var appUserModelId = GetAppUserModelIdByPid(processId);
                var shellItem = GetShellItemForAppByAumid(appUserModelId);

                appInfo.BackgroundColor = shellItem.GetUInt32(ref PropertyKeys.PKEY_AppUserModel_Background);
                appInfo.PackageInstallPath = shellItem.GetString(ref PropertyKeys.PKEY_AppUserModel_PackageInstallPath);
                appInfo.ExeName = appInfo.PackageInstallPath;

                try
                {
                    var rawSmallLogoPath = shellItem.GetString(ref PropertyKeys.PKEY_Tile_SmallLogoPath);
                    var smallLogoPath = Path.Combine(appInfo.PackageInstallPath, rawSmallLogoPath);
                    if (File.Exists(smallLogoPath))
                    {
                        appInfo.SmallLogoPath = smallLogoPath;
                    }
                    else
                    {
                        var mrtResourceManager = (IMrtResourceManager)new MrtResourceManager();
                        mrtResourceManager.InitializeForPackage(shellItem.GetString(ref PropertyKeys.PKEY_AppUserModel_PackageFullName));
                        var map = mrtResourceManager.GetMainResourceMap();
                        appInfo.SmallLogoPath = Path.Combine(appInfo.PackageInstallPath, map.GetFilePath(rawSmallLogoPath));
                    }
                }
                catch(Exception ex)
                {
                    Trace.TraceError($"{ex}");
                }
            }
            else
            {
                appInfo.IsDesktopApp = true;

                var handle = Kernel32.OpenProcess(Kernel32.ProcessFlags.PROCESS_QUERY_LIMITED_INFORMATION | Kernel32.ProcessFlags.SYNCHRONIZE, false, processId);
                if (handle != IntPtr.Zero)
                {
                    try
                    {
                        CheckProcessHandle(processId, handle);

                        var fileNameBuilder = new StringBuilder(260);
                        uint bufferLength = (uint)fileNameBuilder.Capacity;
                        if (Kernel32.QueryFullProcessImageName(handle, 0, fileNameBuilder, ref bufferLength) != 0)
                        {
                            if (fileNameBuilder.Length > 0)
                            {
                                var processFullPath = fileNameBuilder.ToString();
                                appInfo.ExeName = Path.GetFileNameWithoutExtension(processFullPath);
                                appInfo.SmallLogoPath = processFullPath;
                                appInfo.PackageInstallPath = processFullPath;
                            }
                        }
                    }
                    finally
                    {
                        Kernel32.CloseHandle(handle);
                    }
                }
                else
                {
                    if (TryGetExecutableNameViaNtByPid(processId, out appInfo.ExeName))
                    {
                        appInfo.CanTrack = false;
                    }
                    else
                    {
                        throw new ZombieProcessException(processId);
                    }
                }
            }

            return appInfo;
        }

        private static bool TryGetExecutableNameViaNtByPid(int processId, out string executableName)
        {
            bool executableNameRetrieved = false;
            executableName = "";

            var ntstatus = Ntdll.NtQuerySystemInformationInitial(
                Ntdll.SYSTEM_INFORMATION_CLASS.SystemProcessInformation,
                IntPtr.Zero,
                0,
                out int requiredBufferLength);

            if(ntstatus == Ntdll.NTSTATUS.STATUS_INFO_LENGTH_MISMATCH)
            {
                var buffer = Marshal.AllocHGlobal(requiredBufferLength);
                ntstatus = Ntdll.NtQuerySystemInformation(
                    Ntdll.SYSTEM_INFORMATION_CLASS.SystemProcessInformation,
                    buffer,
                    requiredBufferLength,
                    IntPtr.Zero);

                if (ntstatus == Ntdll.NTSTATUS.SUCCESS)
                {
                    Ntdll.SYSTEM_PROCESS_INFORMATION processInfo;
                    IntPtr entryPtr = buffer;
                    do
                    {
                        processInfo = Marshal.PtrToStructure<Ntdll.SYSTEM_PROCESS_INFORMATION>(entryPtr);
                        if (processInfo.UniqueProcessId == processId && processInfo.ImageName.Buffer != IntPtr.Zero)
                        {
                            executableName = Marshal.PtrToStringUni(processInfo.ImageName.Buffer, processInfo.ImageName.Length / 2);
                            executableNameRetrieved = true;
                            break;
                        }
                        entryPtr += processInfo.NextEntryOffset;
                    } while (processInfo.NextEntryOffset != 0);
                }

                Marshal.FreeHGlobal(buffer);
            }

            return executableNameRetrieved;
        }

        internal static string GetDisplayNameForAppByPid(int processId)
        {
            if (processId == 0)
            {
                return null;
            }

            if (IsImmersiveProcess(processId))
            {
                var aumid = GetAppUserModelIdByPid(processId);
                var shellItem = GetShellItemForAppByAumid(aumid);
                return shellItem.GetString(ref PropertyKeys.PKEY_ItemNameDisplay);
            }
            else
            {
                try
                {
                    using (var proc = Process.GetProcessById(processId))
                    {
                        return proc.MainWindowTitle;
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"{ex}");
                }
                return null;
            }
        }

        private static bool IsImmersiveProcess(int processId)
        {
            var processHandle = Kernel32.OpenProcess(Kernel32.ProcessFlags.PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
            if (processHandle == IntPtr.Zero)
            {
                return false;
            }

            try
            {
                return User32.IsImmersiveProcess(processHandle) != 0;
            }
            finally
            {
                Kernel32.CloseHandle(processHandle);
            }
        }

        private static void CheckProcessHandle(int processId, IntPtr handle)
        {
            if (Kernel32.WaitForSingleObject(handle, 0) != Kernel32.WAIT_TIMEOUT)
            {
                throw new ZombieProcessException(processId);
            }
        }

        private static bool CanResolveAppByApplicationUserModelId(string aumid)
        {
            try
            {
                var iid = typeof(IShellItem2).GUID;
                Shell32.SHCreateItemInKnownFolder(ref FolderIds.AppsFolder, Shell32.KF_FLAG_DONT_VERIFY, aumid, ref iid);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
                return false;
            }
        }

        private static string GetAppUserModelIdByPid(int processId)
        {
            string appUserModelId = string.Empty;

            var processHandle = Kernel32.OpenProcess(Kernel32.ProcessFlags.PROCESS_QUERY_LIMITED_INFORMATION | Kernel32.ProcessFlags.SYNCHRONIZE, false, processId);
            if (processHandle != IntPtr.Zero)
            {
                try
                {
                    CheckProcessHandle(processId, processHandle);

                    int amuidBufferLength = Kernel32.MAX_AUMID_LEN;
                    var amuidBuffer = new StringBuilder(amuidBufferLength);

                    Kernel32.GetApplicationUserModelId(processHandle, ref amuidBufferLength, amuidBuffer);
                    appUserModelId = amuidBuffer.ToString();
                }
                finally
                {
                    Kernel32.CloseHandle(processHandle);
                }

                // We may receive an AUMID for an app in a package that doesn't have
                // the metadata we need (e.g. Skype). If the AUMID doesn't resolve to
                // an app, we need to bust open the package, find the main app, and
                // retrieve the metadata we need.
                if (!CanResolveAppByApplicationUserModelId(appUserModelId))
                {
                    int packageFamilyNameLength = Kernel32.PACKAGE_FAMILY_NAME_MAX_LENGTH_INCL_Z;
                    var packageFamilyNameBuilder = new StringBuilder(packageFamilyNameLength);

                    int packageRelativeApplicationIdLength = Kernel32.PACKAGE_RELATIVE_APPLICATION_ID_MAX_LENGTH_INCL_Z;
                    var packageRelativeApplicationIdBuilder = new StringBuilder(packageRelativeApplicationIdLength);

                    Kernel32.ParseApplicationUserModelId(
                        appUserModelId,
                        ref packageFamilyNameLength,
                        packageFamilyNameBuilder,
                        ref packageRelativeApplicationIdLength,
                        packageRelativeApplicationIdBuilder);

                    string packageFamilyName = packageFamilyNameBuilder.ToString();

                    int packageCount = 0;
                    int packageNamesBufferLength = 0;
                    Kernel32.FindPackagesByPackageFamilyInitial(
                        packageFamilyName,
                        Kernel32.PACKAGE_FILTER_HEAD | Kernel32.PACKAGE_INFORMATION_BASIC,
                        ref packageCount,
                        IntPtr.Zero,
                        ref packageNamesBufferLength,
                        IntPtr.Zero,
                        IntPtr.Zero);

                    if (packageCount > 0)
                    {
                        var pointers = new IntPtr[packageCount];
                        IntPtr buffer = Marshal.AllocHGlobal(packageNamesBufferLength * Kernel32.SIZEOF_WCHAR);

                        Kernel32.FindPackagesByPackageFamily(
                            packageFamilyName,
                            Kernel32.PACKAGE_FILTER_HEAD | Kernel32.PACKAGE_INFORMATION_BASIC,
                            ref packageCount,
                            pointers,
                            ref packageNamesBufferLength,
                            buffer,
                            IntPtr.Zero);

                        var packageFullName = Marshal.PtrToStringUni(pointers[0]);
                        Marshal.FreeHGlobal(buffer);

                        Kernel32.OpenPackageInfoByFullName(packageFullName, 0, out IntPtr packageInfoReference);

                        int bufferLength = 0;
                        Kernel32.GetPackageApplicationIds(packageInfoReference, ref bufferLength, IntPtr.Zero, out int appIdCount);

                        buffer = Marshal.AllocHGlobal(bufferLength);
                        Kernel32.GetPackageApplicationIds(packageInfoReference, ref bufferLength, buffer, out appIdCount);

                        appUserModelId = Marshal.PtrToStringUni(Marshal.ReadIntPtr(buffer));
                        Marshal.FreeHGlobal(buffer);

                        Kernel32.ClosePackageInfo(packageInfoReference);
                    }
                }
            }
            else
            {
                throw new ZombieProcessException(processId);
            }

            return appUserModelId;
        }

        private static AppInformation GetInformationForSystemProcess()
        {
            return new AppInformation()
            {
                ExeName = "*SystemSounds",
                BackgroundColor = 0x000000,
                PackageInstallPath = "System.SystemSoundsSession",
                IsDesktopApp = true,
                CanTrack = false,
                SmallLogoPath = Environment.ExpandEnvironmentVariables(
                    $"%windir%\\{(Environment.Is64BitOperatingSystem ? "sysnative" : "system32")}\\audiosrv.dll")
            };
        }
    }
}
