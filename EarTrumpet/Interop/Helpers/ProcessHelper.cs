using System;
using System.Diagnostics;

namespace EarTrumpet.Interop.Helpers
{
    public class ProcessHelper
    {
        public  static void StartNoThrow(string fileName)
        {
            try
            {
                Trace.WriteLine($"ProcessHelper StartNoThrow {fileName}");
                using (Process.Start(fileName))
                {
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ProcessHelper StartNoThrow Failed: {ex}");
            }
        }
    }
}
