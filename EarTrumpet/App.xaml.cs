using EarTrumpet.Services;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using Windows.ApplicationModel;

namespace EarTrumpet
{
    public partial class App
    {
        private Mutex _mMutex;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var mutexName = string.Format(CultureInfo.InvariantCulture, "Local\\{{{0}}}{{{1}}}", assembly.GetType().GUID, assembly.GetName().Name);

            _mMutex = new Mutex(true, mutexName, out bool mutexCreated);
            if (!mutexCreated)
            {
                _mMutex = null;
                Current.Shutdown();
                return;
            }

            WhatsNewDisplayService.ShowIfAppropriate();
            FirstRunDisplayService.ShowIfAppropriate();

            new MainWindow();

#if DEBUG
            if (Debugger.IsAttached)
            {
                new DebugWindow().Show();
            }
#endif
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (_mMutex == null) return;
            _mMutex.ReleaseMutex();
            _mMutex.Close();
            _mMutex = null;
        }

        static bool? _hasIdentity = null;
        internal static bool HasIdentity
        {
            get
            {
#if DEBUG
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
}
