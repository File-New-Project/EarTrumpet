namespace EarTrumpet.Interop
{
    enum SIGDN : uint
    {                                                 // lower word (& with 0xFFFF)
        NORMALDISPLAY = 0x00000000,                   // SHGDN_NORMAL
        PARENTRELATIVEPARSING = 0x80018001,           // SHGDN_INFOLDER | SHGDN_FORPARSING
        DESKTOPABSOLUTEPARSING = 0x80028000,          // SHGDN_FORPARSING
        PARENTRELATIVEEDITING = 0x80031001,           // SHGDN_INFOLDER | SHGDN_FOREDITING
        DESKTOPABSOLUTEEDITING = 0x8004c000,          // SHGDN_FORPARSING | SHGDN_FORADDRESSBAR
        FILESYSPATH = 0x80058000,                     // SHGDN_FORPARSING
        URL = 0x80068000,                             // SHGDN_FORPARSING
        PARENTRELATIVEFORADDRESSBAR = 0x8007c001,     // SHGDN_INFOLDER | SHGDN_FORPARSING | SHGDN_FORADDRESSBAR
        PARENTRELATIVE = 0x80080001,                  // SHGDN_INFOLDER
    }
}
