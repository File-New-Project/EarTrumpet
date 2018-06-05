using Bugsnag;
using Bugsnag.Clients;
using EarTrumpet.Extensions;
using EarTrumpet.Misc;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace EarTrumpet.Services
{
    class ErrorReportingService
    {
        internal static void Initialize()
        {
            try
            {
#if DEBUG
                WPFClient.Config.ApiKey = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\eartrumpet.bugsnag.apikey");
#endif

                WPFClient.Config.StoreOfflineErrors = true;
                WPFClient.Config.AppVersion = App.Current.HasIdentity() ? Package.Current.Id.Version.ToVersionString() : "DevInternal";
                WPFClient.Start();

                WPFClient.Config.BeforeNotify(OnBeforeNotify);

                Task.Factory.StartNew(WPFClient.SendStoredReports);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private static bool OnBeforeNotify(Event error)
        {
            // Remove default metadata we don't need nor want.
            error.Metadata.AddToTab("Device", "machineName", "<redacted>");
            error.Metadata.AddToTab("Device", "hostname", "<redacted>");

            error.Metadata.AddToTab("AppSettings", "IsLightTheme", GetNoError(() => SystemSettings.IsLightTheme));
            error.Metadata.AddToTab("AppSettings", "IsRTL", GetNoError(() => SystemSettings.IsRTL));
            error.Metadata.AddToTab("AppSettings", "IsTransparencyEnabled", GetNoError(() => SystemSettings.IsTransparencyEnabled));
            error.Metadata.AddToTab("AppSettings", "UseAccentColor", GetNoError(() => SystemSettings.UseAccentColor));

            return true;
        }

        private static string GetNoError(Func<object> get)
        {
            try
            {
                var ret = get();
                return ret == null ? "null" : ret.ToString();
            }
            catch (Exception ex)
            {
                return $"{ex}";
            }
        }
    }
}
