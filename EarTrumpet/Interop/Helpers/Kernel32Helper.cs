using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.Helpers
{
    class Kernel32Helper
    {
        public static bool IsPackagedProcess(int processId)
        {
            var processHandle = Kernel32.OpenProcess(Kernel32.ProcessFlags.PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
            if (processHandle == IntPtr.Zero)
            {
                return false;
            }

            var isPackagedProcess = false;
            try
            {
                var bufferSize = 0;
                if (Kernel32.GetPackageId(processHandle, ref bufferSize, IntPtr.Zero) == HRESULT.ERROR_INSUFFICIENT_BUFFER)
                {
                    var packageIdPtr = Marshal.AllocHGlobal(bufferSize);
                    if(Kernel32.GetPackageId(processHandle, ref bufferSize, packageIdPtr) == HRESULT.S_OK)
                    {
                        var packageId = Marshal.PtrToStructure<Kernel32.PACKAGE_ID>(packageIdPtr);
                        isPackagedProcess = packageId.publisher.Length > 0;
                    }
                    Marshal.FreeHGlobal(packageIdPtr);
                }
            }
            finally
            {
                Kernel32.CloseHandle(processHandle);
            }
            return isPackagedProcess;
        }
    }
}
