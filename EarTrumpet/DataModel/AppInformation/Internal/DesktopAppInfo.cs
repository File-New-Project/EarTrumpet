using EarTrumpet.Interop;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.UI.Shell;
using Windows.Win32.System.Threading;

namespace EarTrumpet.DataModel.AppInformation.Internal
{
    class DesktopAppInfo : IAppInfo
    {
        public event Action<IAppInfo> Stopped;

        public string ExeName { get; }
        public string DisplayName { get; }
        public string PackageInstallPath { get; }
        public string SmallLogoPath { get; }
        public bool IsDesktopApp => true;

        private readonly uint _processId;

        public DesktopAppInfo(uint processId, bool trackProcess)
        {
            _processId = processId;

            var handle = PInvoke.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION | PROCESS_ACCESS_RIGHTS.PROCESS_SYNCHRONIZE, false, processId);
            if (handle != IntPtr.Zero)
            {
                try
                {
                    ZombieProcessException.ThrowIfZombie(processId, handle);

                    Span<char> fileName = stackalloc char[260];
                    unsafe
                    {
                        fixed (char* fileNamePtr = fileName)
                        {
                            var fileNameBufferSize = (uint)fileName.Length;
                            if (PInvoke.QueryFullProcessImageName(handle, PROCESS_NAME_FORMAT.PROCESS_NAME_WIN32, fileNamePtr, &fileNameBufferSize) != 0)
                            {
                                if (fileNameBufferSize > 0)
                                {
                                    var processFullPath = new PWSTR(fileNamePtr).ToString();
                                    ExeName = Path.GetFileNameWithoutExtension(processFullPath);
                                    SmallLogoPath = processFullPath;
                                    PackageInstallPath = processFullPath;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    PInvoke.CloseHandle(handle);
                }
            }
            else
            {
                trackProcess = false;

                if (TryGetExecutableNameViaNtByPid(processId, out var imageName))
                {
                    ExeName = Path.GetFileNameWithoutExtension(imageName);
                    SmallLogoPath = imageName;
                    PackageInstallPath = imageName;
                }
                else
                {
                    throw new ZombieProcessException(processId);
                }
            }

            // Display Name priority:
            // - AppsFolder
            // - A window caption
            // - Exe Name

            try
            {
                var appResolver = (IApplicationResolver)new ApplicationResolver();
                appResolver.GetAppIDForProcess((uint)processId, out var appId, out _, out _, out _);
                Marshal.ReleaseComObject(appResolver);

                PInvoke.SHCreateItemInKnownFolder(
                    PInvoke.FOLDERID_AppsFolder,
                    KNOWN_FOLDER_FLAG.KF_FLAG_DONT_VERIFY,
                    appId,
                    typeof(IShellItem2).GUID,
                    out var rawShellItem);

                var shellItem = (IShellItem2)rawShellItem;
                if (shellItem != null)
                {
                    var pkey = PInvoke.PKEY_ItemNameDisplay;
                    var displayNamePtr = new PWSTR();
                    unsafe
                    {
                        shellItem.GetString(&pkey, &displayNamePtr);
                    }
                    DisplayName = displayNamePtr.ToString();
                }
            }
            catch (COMException ex)
            {
                Trace.WriteLine($"DesktopAppInfo DisplayName read failed {ExeName} 0x{((uint)ex.HResult).ToString("x", CultureInfo.CurrentCulture)}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"DesktopAppInfo DisplayName read failed {ExeName} {ex}");
            }

            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                try
                {
                    using var proc = Process.GetProcessById((int)_processId);
                    DisplayName = proc.MainWindowTitle;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }

            if (trackProcess)
            {
                ProcessWatcherService.WatchProcess(processId, (pid) => Stopped?.Invoke(this));
            }
        }

        private static bool TryGetExecutableNameViaNtByPid(uint processId, out string executableName)
        {
            var executableNameRetrieved = false;
            executableName = "";

            var ntstatus = Ntdll.NtQuerySystemInformationInitial(
                Ntdll.SYSTEM_INFORMATION_CLASS.SystemProcessInformation,
                IntPtr.Zero,
                0,
                out var requiredBufferLength);

            if (ntstatus == Ntdll.NTSTATUS.STATUS_INFO_LENGTH_MISMATCH)
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
                    var entryPtr = buffer;
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
    }
}
