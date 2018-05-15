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
#if DEBUG
            // TODO: Remove before ship to make sure UWP debugging is possible.
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
                catch (InvalidOperationException)
                {
                    _hasIdentity = false;
                }
            }

            return (bool)_hasIdentity;
        }
    }
}
