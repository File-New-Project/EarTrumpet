using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Storage.Packaging.Appx;

namespace EarTrumpet.Interop.Helpers;

internal class Kernel32Helper
{
    public static bool IsPackagedProcess(uint processId)
    {
        var processHandle = PInvoke.OpenProcess(Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
        if (processHandle == IntPtr.Zero)
        {
            return false;
        }

        var isPackagedProcess = false;
        try
        {
            unsafe
            {
                uint bufferSize = 0;
                if (PInvoke.GetPackageId(processHandle, &bufferSize) == WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER)
                {
                    var packageIdPtr = Marshal.AllocHGlobal((int)bufferSize);
                    if (PInvoke.GetPackageId(processHandle, &bufferSize, (byte*)packageIdPtr) == WIN32_ERROR.ERROR_SUCCESS)
                    {
                        var packageId = Marshal.PtrToStructure<PACKAGE_ID>(packageIdPtr);
                        isPackagedProcess = packageId.publisher.Length > 0;
                    }
                    Marshal.FreeHGlobal(packageIdPtr);
                }
            }
        }
        finally
        {
            _ = PInvoke.CloseHandle(processHandle);
        }
        return isPackagedProcess;
    }
}
