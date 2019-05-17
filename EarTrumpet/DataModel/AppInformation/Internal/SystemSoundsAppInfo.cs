using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using System;
using System.Diagnostics;

namespace EarTrumpet.DataModel.AppInformation.Internal
{
    class SystemSoundsAppInfo : IAppInfo
    {
        public event Action<IAppInfo> Stopped { add { } remove { } }
        public uint BackgroundColor => 0x000000;
        public string ExeName => "*SystemSounds";
        public string DisplayName => null;
        public string PackageInstallPath => "System.SystemSoundsSession";
        public bool IsDesktopApp => true;
        public string SmallLogoPath { get; set; }

        public SystemSoundsAppInfo()
        {
            SmallLogoPath = Environment.ExpandEnvironmentVariables(Is64BitOperatingSystem() ? 
                @"%windir%\sysnative\audiosrv.dll,203" : @"%windir%\system32\audiosrv.dll,203");
        }

        private static bool Is64BitOperatingSystem()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                return true; // Shortcut for AMD64 machines
            }

            bool is64bit = false;
            if (Environment.OSVersion.IsAtLeast(OSVersions.RS3))
            {
                if (Kernel32.IsWow64Process2(Process.GetCurrentProcess().Handle,
                    out Kernel32.IMAGE_FILE_MACHINE processMachine,
                    out Kernel32.IMAGE_FILE_MACHINE nativeMachine))
                {
                    is64bit =
                        nativeMachine == Kernel32.IMAGE_FILE_MACHINE.IMAGE_FILE_MACHINE_AMD64 ||
                        nativeMachine == Kernel32.IMAGE_FILE_MACHINE.IMAGE_FILE_MACHINE_ARM64;
                }
            }

            return is64bit;
        }
    }
}
