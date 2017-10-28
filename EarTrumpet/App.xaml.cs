using EarTrumpet.Services;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace EarTrumpet
{
    public partial class App
    {
        private Mutex _mMutex;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            bool mutexCreated;
            var mutexName = string.Format(CultureInfo.InvariantCulture, "Local\\{{{0}}}{{{1}}}", assembly.GetType().GUID, assembly.GetName().Name);

            _mMutex = new Mutex(true, mutexName, out mutexCreated);
            
            if (!mutexCreated)
            {
                _mMutex = null;
                Current.Shutdown();
                return;
            }

            WhatsNewDisplayService.ShowIfAppropriate();
            FirstRunDisplayService.ShowIfAppropriate();

            new MainWindow();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (_mMutex == null) return;
            _mMutex.ReleaseMutex();
            _mMutex.Close();
            _mMutex = null;
        }
    }
}
