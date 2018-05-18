using System;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    [Flags]
    public enum ERole
    {
        eConsole = 0,
        eMultimedia = 1,
        eCommunications = 2,
        ERole_enum_count = 3
    }
}