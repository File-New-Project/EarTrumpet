using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EarTrumpet.Extensions
{
    public static class DispatcherExtensions
    {
        public static void SafeInvoke(this Dispatcher dispatcher, Action method)
        {
            if (dispatcher.Thread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                method();
            }
            else
            {
                dispatcher.BeginInvoke(method);
            }
        }
    }
}
