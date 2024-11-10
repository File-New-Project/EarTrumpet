using System;
using System.Diagnostics;
using EarTrumpet.Extensions;
using Windows.Win32;
using Windows.Win32.System.SystemInformation;

namespace EarTrumpet.DataModel.AppInformation.Internal;

internal class SystemSoundsAppInfo : IAppInfo
{
    public event Action<IAppInfo> Stopped { add { } remove { } }
    public static uint BackgroundColor => 0x000000;
    public string ExeName => "*SystemSounds";
    public string DisplayName => null;
    public string PackageInstallPath => "System.SystemSoundsSession";
    public bool IsDesktopApp => true;
    public string SmallLogoPath { get; set; }
    public string AppId => PackageInstallPath;

    public SystemSoundsAppInfo()
    {
        SmallLogoPath = Environment.ExpandEnvironmentVariables(Is64BitOperatingSystem() && !Environment.Is64BitProcess ? 
            @"%windir%\sysnative\audiosrv.dll,203" : @"%windir%\system32\audiosrv.dll,203");
    }

    private static bool Is64BitOperatingSystem()
    {
        if (Environment.Is64BitOperatingSystem)
        {
            return true; // Shortcut for AMD64 machines
        }

        var is64bit = false;
        var nativeMachine = IMAGE_FILE_MACHINE.IMAGE_FILE_MACHINE_UNKNOWN;
        if (Environment.OSVersion.IsAtLeast(OSVersions.RS3))
        {
            unsafe
            {
                if (PInvoke.IsWow64Process2(new HANDLE(Process.GetCurrentProcess().Handle.ToPointer()),
                    null,
                    &nativeMachine))
                {
                    is64bit =
                        nativeMachine == IMAGE_FILE_MACHINE.IMAGE_FILE_MACHINE_AMD64 ||
                        nativeMachine == IMAGE_FILE_MACHINE.IMAGE_FILE_MACHINE_ARM64;
                }
            }
        }

        return is64bit;
    }
}
