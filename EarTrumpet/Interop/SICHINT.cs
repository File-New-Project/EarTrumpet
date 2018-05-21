using System;

namespace EarTrumpet.Interop
{
    [Flags]
    enum SICHINT : uint
    {
        /// <summary>iOrder based on display in a folder view</summary>
        DISPLAY = 0x00000000,
        /// <summary>exact instance compare</summary>
        ALLFIELDS = 0x80000000,
        /// <summary>iOrder based on canonical name (better performance)</summary>
        CANONICAL = 0x10000000,
        /// <summary>Windows 7 and later.</summary>
        TEST_FILESYSPATH_IF_NOT_EQUAL = 0x20000000,
    };
}
