using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    [Guid("CA286FC3-91FD-42C3-8E9B-CAAFA66242E3")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPolicyConfig_TH1
    {
        void Unused1();
        void Unused2();
        void Unused3();
        void Unused4();
        void Unused5();
        void Unused6();
        void Unused7();
        void Unused8();
        void Unused9();
        void Unused10();
        void SetDefaultEndpoint(string wszDeviceId, uint eRole);
    }

    [Guid("6BE54BE8-A068-4875-A49D-0C2966473B11")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPolicyConfig_TH2
    {
        void Unused1();
        void Unused2();
        void Unused3();
        void Unused4();
        void Unused5();
        void Unused6();
        void Unused7();
        void Unused8();
        void Unused9();
        void Unused10();
        void SetDefaultEndpoint(string wszDeviceId, uint eRole);
    }

    [Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPolicyConfig_RS1
    {
        void Unused1();
        void Unused2();
        void Unused3();
        void Unused4();
        void Unused5();
        void Unused6();
        void Unused7();
        void Unused8();
        void Unused9();
        void Unused10();
        void SetDefaultEndpoint(string wszDeviceId, uint eRole);
    }
}
