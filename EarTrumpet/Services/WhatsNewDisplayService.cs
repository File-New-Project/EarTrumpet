using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;

namespace EarTrumpet.Services
{
    public static class WhatsNewDisplayService
    {
        internal static void ShowIfAppropriate()
        {
            var currentVersion = PackageVersionToReadableString(Package.Current.Id.Version);

            var lastVersion = Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(currentVersion)];

            if (lastVersion == null || currentVersion != (string)lastVersion)
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(currentVersion)] = currentVersion;

                System.Diagnostics.Process.Start("eartrumpet:welcome");
            }            
        }

        private static string PackageVersionToReadableString(PackageVersion packageVersion)
        {
            return $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
        }
    }
}
