using System;
using System.Diagnostics;

namespace EarTrumpet.UI.Helpers
{
    class ProcessHelper
    {
        internal static void StartNoThrow(string fileName)
        {
            try
            {
                using (Process.Start(fileName))
                {
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{ex}");
            }
        }
    }
}
