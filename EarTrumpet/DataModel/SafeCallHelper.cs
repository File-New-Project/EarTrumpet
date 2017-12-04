using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.DataModel
{
    public static class SafeCallHelper
    {
        public static T GetValue<T>(Func<T> call)
        {
            try
            {
                return call();
            }
            catch(COMException ex) when ((uint)ex.HResult == 0x88890004)
            {
                // Device is invalidated but the object is still alive. Ignore.
            }
            return default(T);
        }

        public static void SetValue(Action call)
        {
            GetValue(() =>
            {
                call();
                return 0;
            });
        }
    }
}
