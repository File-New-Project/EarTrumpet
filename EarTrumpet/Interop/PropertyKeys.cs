using System;

namespace EarTrumpet.Interop
{
    public static class PropertyKeys
    {
        public static PROPERTYKEY PKEY_ItemNameDisplay = new PROPERTYKEY
        {
            fmtid = new Guid("{B725F130-47EF-101A-A5F1-02608C9EEBAC}"),
            pid = new UIntPtr(10)
        };

        public static PROPERTYKEY PKEY_AppUserModel_Background = new PROPERTYKEY
        {
            fmtid = Guid.Parse("{86D40B4D-9069-443C-819A-2A54090DCCEC}"),
            pid = new UIntPtr(4)
        };

        public static PROPERTYKEY PKEY_AppUserModel_PackageInstallPath = new PROPERTYKEY
        {
            fmtid = Guid.Parse("{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}"),
            pid = new UIntPtr(15)
        };

        public static PROPERTYKEY PKEY_Tile_SmallLogoPath = new PROPERTYKEY
        {
            fmtid = Guid.Parse("{86D40B4D-9069-443C-819A-2A54090DCCEC}"),
            pid = new UIntPtr(2)
        };

        public static PROPERTYKEY PKEY_AppUserModel_PackageFullName = new PROPERTYKEY
        {
            fmtid = Guid.Parse("{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}"),
            pid = new UIntPtr(21)
        };
    }
}
