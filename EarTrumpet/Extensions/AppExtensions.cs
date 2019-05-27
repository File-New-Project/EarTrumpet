using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Windows.ApplicationModel;
using EarTrumpet.Diagnosis;

namespace EarTrumpet.Extensions
{
    public static class AppExtensions
    {
        public static Version GetVersion(this Application app)
        {
            if (HasIdentity(app))
            {
                var packageVer = Package.Current.Id.Version;
                return new Version(packageVer.Major, packageVer.Minor, packageVer.Build, packageVer.Revision);
            }
            else
            {
#if DEBUG
                var versionStr = new StreamReader(Application.GetResourceStream(new Uri("pack://application:,,,/EarTrumpet;component/Assets/DevVersion.txt")).Stream).ReadToEnd();
                return Version.Parse(versionStr);
#else
                return new Version(0, 0, 0, 0);
#endif
            }
        }

        static bool? _hasIdentity = null;
        public static bool HasIdentity(this Application app)
        {
#if VSDEBUG
            if (Debugger.IsAttached)
            {
                return false;
            }
#endif

            if (_hasIdentity == null)
            {
                try
                {
                    _hasIdentity = (Package.Current.Id != null);
                }
                catch (InvalidOperationException ex)
                {
                    _hasIdentity = false;
#if !DEBUG
                    // We do not expect this to occur in production when the app is packaged.
                    ErrorReporter.LogWarning(ex);
#else
                    Trace.WriteLine($"AppExtensions HasIdentity: False {ex.Message}");
#endif
                }
            }

            return (bool)_hasIdentity;
        }
    }
}
