using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Com
{
    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionEnumerator
    {
        void GetCount(out int SessionCount);
        void GetSession(int SessionCount, out IAudioSessionControl Session);
    }
}