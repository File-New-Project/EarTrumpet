using Bugsnag;
using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace EarTrumpet.UI.Services
{
    class ErrorReportingService
    {
        private static bool s_isAppShuttingDown;
        private static Client s_bugsnagClient;

        internal static void Initialize()
        {
            AppTrace.Initialize(OnWarningException);

            try
            {
                Application.Current.Exit += (_, __) => s_isAppShuttingDown = true;

                s_bugsnagClient = new Client(Bugsnag.ConfigurationSection.Configuration.Settings);
                s_bugsnagClient.BeforeNotify(new Middleware(OnBeforeNotify));
            }
            catch (Exception ex)
            {
                // No point in AppTrace.LogWarning here because bugsnag is broken.
                Trace.WriteLine(ex);
            }
        }

        private static void OnWarningException(Exception ex)
        {
            Trace.WriteLine($"## Warning Notify ##: {ex}");
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                s_bugsnagClient.Notify(ex, Severity.Warning);
            }));
        }

        private static void OnBeforeNotify(Bugsnag.Payload.Report error)
        {
            try
            {
                foreach (var evt in (Bugsnag.Payload.Event[])error["events"])
                {
                    // Remove default properties that we don't need.
                    evt.Device.Clear();

                    evt.Device.Add("osVersionBuild", GetNoError(() => SystemSettings.BuildLabel));
                    evt.Device.Add("osArchitecture", GetNoError(() => Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit"));
                    evt.Device.Add("processorCount", GetNoError(() => Environment.ProcessorCount + " core(s)"));

                    evt.App.Add("version", App.Current.GetVersion().ToString());
#if DEBUG
                    evt.App.Add("releaseStage", "development");
#else
                    evt.App.Add("releaseStage", "production");
#endif

                    var appSettings = new Dictionary<string, string>();
                    evt.Metadata.Add("AppSettings", appSettings);

                    appSettings.Add("IsLightTheme", GetNoError(() => SystemSettings.IsLightTheme));
                    appSettings.Add("IsSystemLightTheme", GetNoError(() => SystemSettings.IsSystemLightTheme));
                    appSettings.Add("IsRTL", GetNoError(() => SystemSettings.IsRTL));
                    appSettings.Add("IsTransparencyEnabled", GetNoError(() => SystemSettings.IsTransparencyEnabled));
                    appSettings.Add("Culture", GetNoError(() => CultureInfo.CurrentCulture.Name));
                    appSettings.Add("CurrentUICulture", GetNoError(() => CultureInfo.CurrentUICulture.Name));
                    appSettings.Add("UseAccentColor", GetNoError(() => SystemSettings.UseAccentColor));
                    appSettings.Add("AnimationsEnabled", GetNoError(() => SystemParameters.MenuAnimation));
                    appSettings.Add("IsShuttingDown", GetNoError(() => s_isAppShuttingDown));
                    appSettings.Add("HasIdentity", GetNoError(() => Application.Current.HasIdentity()));
                    appSettings.Add("TrayIconId", GetNoError(() => ((App)App.Current).TrayViewModel.Id));
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"## ErrorReportingService OnBeforeNotify: {ex}");
            }
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
