using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Extensions;

public static class ExceptionExtensions
{
    public static bool Is(this Exception ex, HRESULT type)
    {
        if (type == HRESULT.AUDCLNT_E_DEVICE_INVALIDATED
            || type == HRESULT.AUDCLNT_S_NO_SINGLE_PROCESS
            || type == HRESULT.ERROR_NOT_FOUND)
        {
            return (uint)(ex as COMException)?.HResult == (uint)type;
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
