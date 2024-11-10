using System;
using Windows.Win32.UI.Shell.PropertiesSystem;

namespace Windows.Win32;

public static partial class PInvoke
{
    // Missing property keys https://github.com/microsoft/win32metadata/issues/1773

    public static readonly PROPERTYKEY PKEY_AppUserModel_Background = new()
    {
        // {86D40B4D-9069-443C-819A-2A54090DCCEC},4
        fmtid = new Guid(0x86d40b4d, 0x9069, 0x443c, 0x81, 0x9a, 0x2a, 0x54, 0x09, 0x0d, 0xcc, 0xec),
        pid = 4
    };

    public static readonly PROPERTYKEY PKEY_AppUserModel_PackageInstallPath = new()
    {
        // {9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3},15
        fmtid = new Guid(0x9f4c2855, 0x9f79, 0x4b39, 0xa8, 0xd0, 0xe1, 0xd4, 0x2d, 0xe1, 0xd5, 0xf3),
        pid = 15
    };

    public static readonly PROPERTYKEY PKEY_Tile_SmallLogoPath = new()
    {
        // {86D40B4D-9069-443C-819A-2A54090DCCEC},2
        fmtid = new Guid(0x86d40b4d, 0x9069, 0x443c, 0x81, 0x9a, 0x2a, 0x54, 0x09, 0x0d, 0xcc, 0xec),
        pid = 2
    };

    public static readonly PROPERTYKEY PKEY_AppUserModel_PackageFullName = new()
    {
        // {9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3},21
        fmtid = new Guid(0x9f4c2855, 0x9f79, 0x4b39, 0xa8, 0xd0, 0xe1, 0xd4, 0x2d, 0xe1, 0xd5, 0xf3),
        pid = 21
    };

    public static readonly PROPERTYKEY PKEY_AudioEndPoint_Interface = new()
    {
        // {A45C254E-DF1C-4EFD-8020-67D146A850E0},2
        fmtid = new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0),
        pid = 2
    };
}
