using EarTrumpet.Interop;
using System;

namespace EarTrumpet.DataModel.AppInformation.Internal
{
    public class ZombieProcessException : Exception
    {
        public ZombieProcessException(int processId) : base($"Process is a zombie: {processId}") { }

        public static void ThrowIfZombie(int processId, IntPtr handle)
        {
            if (Kernel32.WaitForSingleObject(handle, 0) != Kernel32.WAIT_TIMEOUT)
            {
                throw new ZombieProcessException(processId);
            }
        }
    }
}
