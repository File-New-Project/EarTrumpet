using System;
using System.Windows;
using Windows.ApplicationModel;

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
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
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
                    Diagnosis.ErrorReporter.LogWarning(ex);
#else
                    System.Diagnostics.Trace.WriteLine($"AppExtensions HasIdentity: False {ex.Message}");
#endif
                }
            }

            return (bool)_hasIdentity;
        }
    }
}
