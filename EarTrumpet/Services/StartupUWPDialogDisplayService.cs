using EarTrumpet.Extensions;
using System;
using System.Diagnostics;
using Windows.ApplicationModel;

namespace EarTrumpet.Services
{
    // Note: We can't trust the Windows.Storage APIs, so make sure errors are handled in all cases.
    class StartupUWPDialogDisplayService
    {
        private static string FirstRunKey = "hasShownFirstRun";
        private static string CurrentVersionKey = "currentVersion";

        internal static void ShowIfAppropriate()
        {
            Trace.WriteLine($"StartupUWPDialogDisplayService ShowIfAppropriate {App.Current.HasIdentity()}");

            if (App.Current.HasIdentity())
            {
                ShowWhatsNewIfAppropriate();
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

        internal static void ShowWhatsNewIfAppropriate()
        {
            // Show only on Major/Minor build change.
            // 1.9.4.0 Major.Minor.Build.Revision (ignore Build and Revision)
            var binaryVersion = Package.Current.Id.Version;
            var storedVersion = GetStoredVersion();
            if (storedVersion.Major != binaryVersion.Major ||
                storedVersion.Minor != binaryVersion.Minor)
            {
                Set(CurrentVersionKey, binaryVersion.ToVersionString());

                if (HasKey(FirstRunKey))
                {
                    ProtocolLaunchEarTrumpet("changelog");
                }
            }
        }

        private static PackageVersion GetStoredVersion()
        {
            try
            {
                var verStr = Get<string>(CurrentVersionKey);
                if (!string.IsNullOrWhiteSpace(verStr))
                {
                    var ver = Version.Parse(verStr);
                    return new PackageVersion
                    {
                        Major = (ushort)ver.Major,
                        Minor = (ushort)ver.Minor,
                        Build = (ushort)ver.Build,
                        Revision = (ushort)ver.Revision
                    };
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
            }

            return new PackageVersion { Major = 0, Minor = 0, Build = 0, Revision = 0 };
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

        private static bool HasKey(string key)
        {
            try
            {
                return Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
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
            }
            return default(T);
        }
    }
}
