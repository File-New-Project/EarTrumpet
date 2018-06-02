using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Extensions
{
    public enum Error
    {
        AUDCLNT_E_DEVICE_INVALIDATED,
        ERROR_NOT_FOUND,
    }

    public static class ExceptionExtensioncs
    {
        public static bool Is(this Exception ex, Error type)
        {
            switch(type)
            {
                case Error.AUDCLNT_E_DEVICE_INVALIDATED:
                    return (uint)(ex as COMException)?.HResult == 0x88890004;
                case Error.ERROR_NOT_FOUND:
                    return (uint)(ex as COMException)?.HResult == 0x80070490;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
