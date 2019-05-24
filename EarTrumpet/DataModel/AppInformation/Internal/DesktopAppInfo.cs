using EarTrumpet.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace EarTrumpet.DataModel.AppInformation.Internal
{
    class DesktopAppInfo : IAppInfo
    {
        public event Action<IAppInfo> Stopped;

        public uint BackgroundColor { get; }
        public string ExeName { get; }
        public string DisplayName { get; }
        public string PackageInstallPath { get; }
        public string SmallLogoPath { get; }
        public bool IsDesktopApp => true;

        private int _processId;

        public DesktopAppInfo(int processId, bool trackProcess)
        {
            _processId = processId;

            var handle = Kernel32.OpenProcess(Kernel32.ProcessFlags.PROCESS_QUERY_LIMITED_INFORMATION | Kernel32.ProcessFlags.SYNCHRONIZE, false, processId);
            if (handle != IntPtr.Zero)
            {
                try
                {
                    ZombieProcessException.ThrowIfZombie(processId, handle);

                    var fileNameBuilder = new StringBuilder(260);
                    uint bufferLength = (uint)fileNameBuilder.Capacity;
                    if (Kernel32.QueryFullProcessImageName(handle, 0, fileNameBuilder, ref bufferLength) != 0)
                    {
                        if (fileNameBuilder.Length > 0)
                        {
                            var processFullPath = fileNameBuilder.ToString();
                            ExeName = Path.GetFileNameWithoutExtension(processFullPath);
                            SmallLogoPath = processFullPath;
                            PackageInstallPath = processFullPath;
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
                appResolver.GetAppIDForProcess((uint)processId, out string appId, out _, out _, out _);
                Marshal.ReleaseComObject(appResolver);

                var shellItem = Shell32.SHCreateItemInKnownFolder(FolderIds.AppsFolder, Shell32.KF_FLAG_DONT_VERIFY, appId, typeof(IShellItem2).GUID);
                DisplayName = shellItem.GetString(ref PropertyKeys.PKEY_ItemNameDisplay);
            }
            catch (COMException ex)
            {
                Trace.WriteLine($"DesktopAppInfo DisplayName read failed {ExeName} 0x{((uint)ex.HResult).ToString("x")}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"DesktopAppInfo DisplayName read failed {ExeName} {ex}");
            }

            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                try
                {
                    using (var proc = Process.GetProcessById(_processId))
                    {
                        DisplayName = proc.MainWindowTitle;
                    }
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

        private static bool TryGetExecutableNameViaNtByPid(int processId, out string executableName)
        {
            bool executableNameRetrieved = false;
            executableName = "";

            var ntstatus = Ntdll.NtQuerySystemInformationInitial(
                Ntdll.SYSTEM_INFORMATION_CLASS.SystemProcessInformation,
                IntPtr.Zero,
                0,
                out int requiredBufferLength);

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
    }
}
