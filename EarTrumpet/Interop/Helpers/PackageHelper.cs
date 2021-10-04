using System;
using Windows.ApplicationModel;

namespace EarTrumpet.Interop.Helpers
{
    class PackageHelper
    {
        public static Version GetVersion(bool isPackaged)
        {
            if (isPackaged)
            {
                var packageVer = Package.Current.Id.Version;
                return new Version(packageVer.Major, packageVer.Minor, packageVer.Build, packageVer.Revision);
            }
            else
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public static bool CheckHasIdentity()
        {
#if VSDEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return false;
            }
#endif

            try
            {
                return Package.Current.Id != null;
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Trace.WriteLine($"AppExtensions HasIdentity Failed: {ex.Message}");

                // We do not expect this to occur in production when the app is packaged.
                // Async so that the HasIdentity bit is properly set.
#if !DEBUG
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() => Diagnosis.ErrorReporter.LogWarning(ex)));
#endif
                return false;
            }
        }

        public static bool HasDevIdentity()
        {
#if VSDEBUG
            return true;
#else
            bool result = false;
            try
            {
                result = Package.Current.DisplayName.EndsWith("(dev)");
            }
            catch
            {
            }
            return result;
#endif
        }
    }
}
