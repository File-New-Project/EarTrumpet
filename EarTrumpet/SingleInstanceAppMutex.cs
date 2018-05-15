using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EarTrumpet
{
    class SingleInstanceAppMutex
    {
        private static Mutex _mutex;

        public static bool TakeExclusivity()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var mutexName = string.Format(CultureInfo.InvariantCulture, "Local\\{{{0}}}{{{1}}}", assembly.GetType().GUID, assembly.GetName().Name);

            _mutex = new Mutex(true, mutexName, out bool mutexCreated);
            if (!mutexCreated)
            {
                _mutex = null;
                return false;
            }
            return true;
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
