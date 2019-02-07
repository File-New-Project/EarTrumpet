using System;
using System.Diagnostics;
using System.IO;
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
                return Version.Parse(Package.Current.Id.Version.ToVersionString());
            }
            else
            {
#if DEBUG
                var versionStr = new StreamReader(Application.GetResourceStream(new Uri("pack://application:,,,/EarTrumpet;component/Assets/DevVersion.txt")).Stream).ReadToEnd();
                return Version.Parse(versionStr);
#else
                throw new NotImplementedException();
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
#if !DEBUG
                    // We do not expect this to occur in production when the app is packaged.
                    AppTrace.LogWarning(ex);
#else
                    Trace.WriteLine(ex);
#endif
                    _hasIdentity = false;
                }
            }

            return (bool)_hasIdentity;
        }
    }
}
