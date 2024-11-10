using System;
using Windows.Win32;

namespace EarTrumpet.DataModel.AppInformation.Internal;

public class ZombieProcessException(uint processId) : Exception($"Process is a zombie: {processId}")
{
    public static void ThrowIfZombie(uint processId, IntPtr handle)
    {
        unsafe
        {
            if (PInvoke.WaitForSingleObject(new HANDLE(handle.ToPointer()), 0) != WAIT_EVENT.WAIT_TIMEOUT)
            {
                throw new ZombieProcessException(processId);
            }
        }
    }
}
