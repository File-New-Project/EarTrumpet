using System;

namespace EarTrumpet.Extensions
{
    public enum OSVersions : int
    {
        RS3 = 16299,
        RS4 = 17134,
        RS5_1809 = 17763,
        Version21H2 = 21390,
        Windows11 = 22000,
    }

    public static class OperatingSystemExtensions
    {
        public static bool IsAtLeast(this OperatingSystem os, OSVersions version)
        {
            return os.Version.Build >= (int)version;
        }

        public static bool IsGreaterThan(this OperatingSystem os, OSVersions version)
        {
            return os.Version.Build > (int)version;
        }

        public static bool IsLessThan(this OperatingSystem os, OSVersions version)
        {
            return os.Version.Build < (int)version;
        }
    }
}
