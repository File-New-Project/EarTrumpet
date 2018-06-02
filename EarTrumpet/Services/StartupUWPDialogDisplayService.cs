using EarTrumpet.Extensions;
using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.Foundation.Collections;

namespace EarTrumpet.Services
{
    class StartupUWPDialogDisplayService
    {
        private static string FirstRunKey = "hasShownFirstRun";
        private static string CurrentVersionKey = "currentVersion";
        
        private static IPropertySet LocalSettings => Windows.Storage.ApplicationData.Current.LocalSettings.Values;

        internal static void ShowIfAppropriate()
        {
            Trace.WriteLine($"StartupUWPDialogDisplayService ShowIfAppropriate {App.Current.HasIdentity()}");

            if (App.Current.HasIdentity())
            {
                ShowWelcomeIfAppropriate();
                ShowWhatsNewIfAppropriate();

                App.Current.Exit += App_Exit;
            }
        }

        private static void App_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            try
            {
                foreach (var proc in Process.GetProcessesByName("EarTrumpet.UWP"))
                {
                    try
                    {
                        proc.Kill();
                    }
                    catch(Exception ex)
                    {
                        Trace.TraceError($"{ex}");
                    }
                    finally
                    {
                        proc.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
            }
        }

        internal static void ShowWelcomeIfAppropriate()
        {
            if (App.Current.HasIdentity())
            {
                if (!LocalSettings.ContainsKey(FirstRunKey))
                {
                    LocalSettings[FirstRunKey] = true;
                    ProtocolLaunchEarTrumpet("welcome");
                }
            }
        }

        internal static void ShowWhatsNewIfAppropriate()
        {
            var binaryVersion = Package.Current.Id.Version.ToVersionString();
            var storedVersion = LocalSettings[CurrentVersionKey];
            if ((storedVersion == null || binaryVersion != (string)storedVersion))
            {
                LocalSettings[CurrentVersionKey] = binaryVersion;

                if (LocalSettings.ContainsKey(FirstRunKey))
                {
                    ProtocolLaunchEarTrumpet("changelog");
                }
            }
        }

        private static void ProtocolLaunchEarTrumpet(string more = "")
        {
            Trace.WriteLine($"StartupUWPDialogDisplayService ProtocolLaunchEarTrumpet {more}");

            try
            {
                using (Process.Start($"eartrumpet://{more}")) { }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
            }
        }
    }
}
