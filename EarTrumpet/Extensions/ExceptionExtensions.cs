using EarTrumpet.Interop;
using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Extensions
{
    public static class ExceptionExtensions
    {
        public static bool Is(this Exception ex, Error type)
        {
            switch(type)
            {
                case Error.AUDCLNT_E_DEVICE_INVALIDATED:
                case Error.AUDCLNT_S_NO_SINGLE_PROCESS:
                case Error.ERROR_NOT_FOUND:
                    return (uint)(ex as COMException)?.HResult == (uint)type;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
