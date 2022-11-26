using System.Diagnostics.CodeAnalysis;

namespace EarTrumpet.Interop
{
    [SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name")]
    enum CLSCTX : int
    {
        CLSCTX_INPROC_SERVER = 0x1,
    }
}
