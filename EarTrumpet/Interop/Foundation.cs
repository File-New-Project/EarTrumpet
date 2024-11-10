namespace Windows.Win32;

public static partial class PInvoke
{
    // Missing constants https://github.com/microsoft/win32metadata/issues/1777

    public static readonly unsafe PCWSTR RT_ICON = (char*)3;
    public static readonly unsafe PCWSTR RT_GROUP_ICON = (char*)14;
}