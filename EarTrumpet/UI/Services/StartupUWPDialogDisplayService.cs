using EarTrumpet.DataModel.Storage;
using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using System;
using System.Diagnostics;

namespace EarTrumpet.UI.Services
{
    class StartupUWPDialogDisplayService
    {
        private static readonly string FirstRunKey = "hasShownFirstRun";
        private static ISettingsBag s_settings = StorageFactory.GetSettings();

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

        private static void ShowWelcomeIfAppropriate()
        {
            if (!s_settings.HasKey(FirstRunKey))
            {
                s_settings.Set(FirstRunKey, true);
                ProtocolLaunchEarTrumpet("welcome");
            }
        }

        private static void ProtocolLaunchEarTrumpet(string more = "")
        {
            Trace.WriteLine($"StartupUWPDialogDisplayService ProtocolLaunchEarTrumpet {more}");
            ProcessHelper.StartNoThrow($"eartrumpet://{more}");
        }
    }
}
