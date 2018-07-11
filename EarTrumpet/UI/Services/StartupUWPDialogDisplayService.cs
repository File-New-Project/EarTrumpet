using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using System;
using System.Diagnostics;

namespace EarTrumpet.UI.Services
{
    // Note: We can't trust the Windows.Storage APIs, so make sure errors are handled in all cases.
    class StartupUWPDialogDisplayService
    {
        private static string FirstRunKey = "hasShownFirstRun";

        internal static void ShowIfAppropriate()
        {
            Trace.WriteLine($"StartupUWPDialogDisplayService ShowIfAppropriate {App.Current.HasIdentity()}");

            if (App.Current.HasIdentity())
            {
                ShowWelcomeIfAppropriate();

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
                    catch (Exception ex)
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
                if (!HasKey(FirstRunKey))
                {
                    Set(FirstRunKey, true);
                    ProtocolLaunchEarTrumpet("welcome");
                }
            }
        }

        private static void ProtocolLaunchEarTrumpet(string more = "")
        {
            Trace.WriteLine($"StartupUWPDialogDisplayService ProtocolLaunchEarTrumpet {more}");

            using (ProcessHelper.StartNoThrow($"eartrumpet://{more}")) { }
        }

        private static bool HasKey(string key)
        {
            try
            {
                return Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
                AppTrace.LogWarning(ex);
            }
            return false;
        }

        private static void Set<T>(string key, T value)
        {
            try
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = value;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
                AppTrace.LogWarning(ex);
            }
        }

        private static T Get<T>(string key)
        {
            try
            {
                return (T)Windows.Storage.ApplicationData.Current.LocalSettings.Values[key];
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
                AppTrace.LogWarning(ex);
            }
            return default(T);
        }
    }
}
