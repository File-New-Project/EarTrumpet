using EarTrumpet.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace EarTrumpet.DataModel.AppInformation.Internal
{
    class ModernAppInfo : IAppInfo
    {
        public event Action<IAppInfo> Stopped;

        public uint BackgroundColor { get; }
        public string ExeName { get; }
        public string DisplayName { get; }
        public string PackageInstallPath { get; }
        public string SmallLogoPath { get; }
        public bool IsDesktopApp => false;

        private int processId;

        public ModernAppInfo(int processId, bool trackProcess)
        {
            this.processId = processId;

            var appUserModelId = GetAppUserModelIdByPid(processId);
            var shellItem = GetShellItemForAppByAumid(appUserModelId);

            BackgroundColor = shellItem.GetUInt32(ref PropertyKeys.PKEY_AppUserModel_Background);
            PackageInstallPath = shellItem.GetString(ref PropertyKeys.PKEY_AppUserModel_PackageInstallPath);
            ExeName = PackageInstallPath;
            DisplayName = AppsFolder.ReadDisplayName(appUserModelId);
            SmallLogoPath = appUserModelId;

            if (trackProcess)
            {
                ProcessWatcherService.WatchProcess(processId, (pid) => Stopped?.Invoke(this));
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
                    ZombieProcessException.ThrowIfZombie(processId, processHandle);

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

        private static IShellItem2 GetShellItemForAppByAumid(string aumid)
        {
            return Shell32.SHCreateItemInKnownFolder(FolderIds.AppsFolder, Shell32.KF_FLAG_DONT_VERIFY, aumid, typeof(IShellItem2).GUID);
        }

        private static bool CanResolveAppByApplicationUserModelId(string aumid)
        {
            try
            {
                Shell32.SHCreateItemInKnownFolder(FolderIds.AppsFolder, Shell32.KF_FLAG_DONT_VERIFY, aumid, typeof(IShellItem2).GUID);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{ex}");
                return false;
            }
        }
    }
}
