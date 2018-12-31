using System;
using System.Diagnostics;
using System.Windows;
using Windows.ApplicationModel;

namespace EarTrumpet.Extensions
{
    public static class AppExtensions
    {
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
