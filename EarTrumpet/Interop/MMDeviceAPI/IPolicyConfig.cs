using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    // TH1: CA286FC3-91FD-42C3-8E9B-CAAFA66242E3
    // TH2: 6BE54BE8-A068-4875-A49D-0C2966473B11
    [Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPolicyConfigWin7
    {
        void Unused1();
        void Unused2();
        void Unused3();
        void Unused4();
        void Unused5();
        void Unused6();
        void Unused7();
        void Unused8();
        void GetPropertyValue(string wszDeviceId, ref PROPERTYKEY pkey, ref PropVariant pv);
        void SetPropertyValue(string wszDeviceId, ref PROPERTYKEY pkey, ref PropVariant pv);
        void SetDefaultEndpoint(string wszDeviceId, ERole eRole);
        void SetEndpointVisibility(string wszDeviceId, [MarshalAs(UnmanagedType.Bool)]bool isVisible);
    }
}
