using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Interfaces
{
    [Guid("c0ef2098-bf0d-4db3-9d9f-ccb41279db98")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionControlInternal
    {
        void Unused3();
        void Unused4();
        void Unused5();
        void Unused6();
        void Unused7();
        void Unused8();
        void Unused9();
        void Unused10();
        void Unused11();
        void Unused12();
        void Unused13();
        void Unused14();
        void Unused15();
        void Unused16();
        void Unused17();
        void Unused18();
        void GetStreamFlags(out int flags);
    }
}
