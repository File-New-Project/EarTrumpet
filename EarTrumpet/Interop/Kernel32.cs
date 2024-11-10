namespace EarTrumpet.Interop;

internal class Kernel32
{
    // Missing constants https://github.com/microsoft/win32metadata/issues/1776

    internal const uint MAX_AUMID_LEN = 512;
    internal const int PACKAGE_INFORMATION_BASIC = 0x00000000;
    internal const int PACKAGE_FILTER_HEAD = 0x00000010;
    internal const int PACKAGE_FAMILY_NAME_MAX_LENGTH_INCL_Z = 65 * SIZEOF_WCHAR;
    internal const int PACKAGE_RELATIVE_APPLICATION_ID_MAX_LENGTH_INCL_Z = 65 * SIZEOF_WCHAR;
    internal const int SIZEOF_WCHAR = 2;
}
