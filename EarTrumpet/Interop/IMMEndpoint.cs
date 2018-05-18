using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    [Guid("1BE09788-6894-4089-8586-9A2A6C265AC5")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMEndpoint
    {
        void GetDataFlow(out EDataFlow dataFlow);
    }
}
