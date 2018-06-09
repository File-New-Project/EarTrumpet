using Bugsnag;
using Bugsnag.Clients;
using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel;

namespace EarTrumpet.UI.Services
{
    class ErrorReportingService
    {
        internal static void Initialize()
        {
            AppTrace.Initialize(OnWarningException);

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
                // No point in AppTrace.LogWarning here because bugsnag is broken.
                Trace.WriteLine(ex);
            }
        }

        private static void OnWarningException(Exception ex)
        {
            Trace.WriteLine($"BUGSNAG Warning Notify: {ex}");
            WPFClient.Notify(ex, Severity.Warning);
        }

        private static bool OnBeforeNotify(Event error)
        {
            // Remove default metadata we don't need nor want.
            error.Metadata.AddToTab("Device", "machineName", "<redacted>");
            error.Metadata.AddToTab("Device", "hostname", "<redacted>");

            error.Metadata.AddToTab("Device", "osVersionBuild", GetNoError(() => SystemSettings.BuildLabel));

            error.Metadata.AddToTab("AppSettings", "IsLightTheme", GetNoError(() => SystemSettings.IsLightTheme));
            error.Metadata.AddToTab("AppSettings", "IsRTL", GetNoError(() => SystemSettings.IsRTL));
            error.Metadata.AddToTab("AppSettings", "IsTransparencyEnabled", GetNoError(() => SystemSettings.IsTransparencyEnabled));
            error.Metadata.AddToTab("AppSettings", "UseAccentColor", GetNoError(() => SystemSettings.UseAccentColor));
            error.Metadata.AddToTab("AppSettings", "AnimationsEnabled", GetNoError(() => SystemParameters.MenuAnimation));

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
                // NOTE: Do not use AppTrace.LogWarning here - it would go into a loop.
                return $"{ex}";
            }
        }
    }
}
