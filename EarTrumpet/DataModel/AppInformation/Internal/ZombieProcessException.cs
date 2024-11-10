using System;
using Windows.Win32;

namespace EarTrumpet.DataModel.AppInformation.Internal;

public class ZombieProcessException : Exception
{
    public ZombieProcessException(uint processId) : base($"Process is a zombie: {processId}") { }

    public static void ThrowIfZombie(uint processId, IntPtr handle)
    {
        if (PInvoke.WaitForSingleObject(new HANDLE(handle), 0) != WAIT_EVENT.WAIT_TIMEOUT)
        {
            throw new ZombieProcessException(processId);
        }
    }
}
