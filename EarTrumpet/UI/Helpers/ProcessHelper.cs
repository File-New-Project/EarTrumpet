using System;
using System.Diagnostics;

namespace EarTrumpet.UI.Helpers
{
    class ProcessHelper
    {
        internal static IDisposable StartNoThrow(string fileName)
        {
            try
            {
                return Process.Start(fileName);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
            }
            return null;
        }
    }
}
