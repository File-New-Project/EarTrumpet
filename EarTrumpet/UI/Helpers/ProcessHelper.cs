using System;
using System.Diagnostics;

namespace EarTrumpet.UI.Helpers
{
    class ProcessHelper
    {
        internal static IDisposable StartNoThrowAndLogWarning(string fileName)
        {
            try
            {
                return Process.Start(fileName);
            }
            catch (Exception ex)
            {
                AppTrace.LogWarning(ex);
            }
            return null;
        }
    }
}
