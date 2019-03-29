using System;

namespace EarTrumpet.Interop.Helpers
{
    class User32Helper
    {
        public static bool IsImmersiveProcess(int processId)
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
    }
}
