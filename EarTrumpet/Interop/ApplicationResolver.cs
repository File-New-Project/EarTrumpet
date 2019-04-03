using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    [ComImport]
    [Guid("660B90C8-73A9-4B58-8CAE-355B7F55341B")]
    class ApplicationResolver { }

    [ComImport]
    [Guid("DE25675A-72DE-44b4-9373-05170450C140")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IApplicationResolver
    {
        void _();
        void __();
        void ___();
        void GetAppIDForProcess(uint processId, [MarshalAs(UnmanagedType.LPWStr)] out string appId, out IntPtr _, out IntPtr __, out IntPtr ____);
    }
}
