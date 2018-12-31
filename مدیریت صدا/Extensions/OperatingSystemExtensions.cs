using System;

namespace EarTrumpet.Extensions
{
    public enum OSVersions : int
    {
        RS3 = 16299,
        RS4 = 17134,
        RS5_Prerelease = 17723,
    }

    public static class OperatingSystemExtensions
    {
        public static bool IsAtLeast(this OperatingSystem os, OSVersions version)
        {
            return os.Version.Build >= (int)version;
        }
    }
}
