using System;
using System.Runtime.InteropServices;

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
