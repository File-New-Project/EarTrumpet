using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    enum SIIGBF : int
    {
        SIIGBF_RESIZETOFIT = 0,
        SIIGBF_BIGGERSIZEOK = 0x1,
        SIIGBF_MEMORYONLY = 0x2,
        SIIGBF_ICONONLY = 0x4,
        SIIGBF_THUMBNAILONLY = 0x8,
        SIIGBF_INCACHEONLY = 0x10,
        SIIGBF_CROPTOSQUARE = 0x20,
        SIIGBF_WIDETHUMBNAILS = 0x40,
        SIIGBF_ICONBACKGROUND = 0x80,
        SIIGBF_SCALEUP = 0x100,
    }

    [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IShellItemImageFactory
    {
        void GetImage(SIZE size, SIIGBF flags, out IntPtr hBitmap);
    }
}
