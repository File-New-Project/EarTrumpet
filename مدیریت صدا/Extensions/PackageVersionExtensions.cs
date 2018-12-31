using Windows.ApplicationModel;

namespace EarTrumpet.Extensions
{
    public static class PackageVersionExtensions
    {
        public static string ToVersionString(this PackageVersion packageVersion)
        {
            return $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
        }
    }
}
