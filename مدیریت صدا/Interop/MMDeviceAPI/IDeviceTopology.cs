using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    public interface IConnector { }

    [Guid("82149A85-DBA6-4487-86BB-EA8F7FEFCC71")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISubunit { }

    [Guid("2A07407E-6497-4A18-9787-32F79BD0D98F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDeviceTopology
    {
        uint GetConnectorCount();
        object GetConnector(uint index);
        uint GetSubunitCount();
        ISubunit GetSubunit(uint index);
        object GetPartById(uint id);
        void GetDeviceId([MarshalAs(UnmanagedType.LPWStr)] out string ppwstrDeviceId);
        int GetSignalPath();
    }
}
