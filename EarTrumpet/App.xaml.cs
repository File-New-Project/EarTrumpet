using System.Threading;
using System.Windows;
using Castle.Windsor;
using Castle.Windsor.Installer;

namespace EarTrumpet
{
    public partial class App
    {
        private Mutex _mutex;
        private WindsorContainer _container;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (CreateMutex())
            {
                InitializeWindsor();
                _container.Resolve<MainWindow>();
            }
            else
            {
                Current.Shutdown();
            }
        }

        private void InitializeWindsor()
        {
            _container = new WindsorContainer();
            _container.Install(FromAssembly.This());
        }

        private bool CreateMutex()
        {
            bool mutexCreated;

            #if DEBUG
            _mutex = new Mutex(true, @"Local\EarTrumpet_Debug", out mutexCreated);
            #else
            _mutex = new Mutex(true, @"Local\EarTrumpet", out mutexCreated);
            #endif

            return mutexCreated;
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Close();
            }
        }
    }
}
