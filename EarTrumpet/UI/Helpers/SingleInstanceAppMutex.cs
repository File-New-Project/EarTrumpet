using System.Globalization;
using System.Reflection;
using System.Threading;

namespace EarTrumpet.UI.Helpers
{
    class SingleInstanceAppMutex 
    {
        private static Mutex _mutex;

        public static bool TakeExclusivity()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var mutexName = $"Local\\{assembly.GetName().Name}-0e510f7b-aed2-40b0-ad72-d2d3fdc89a02";

            _mutex = new Mutex(true, mutexName, out bool mutexCreated);
            if (!mutexCreated)
            {
                _mutex = null;
                return false;
            }

            App.Current.Exit += App_Exit;

            return true;
        }

        private static void App_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            ReleaseExclusivity();
        }

        public static void ReleaseExclusivity()
        {
            if (_mutex == null) return;
            _mutex.ReleaseMutex();
            _mutex.Close();
            _mutex = null;
        }
    }
}
