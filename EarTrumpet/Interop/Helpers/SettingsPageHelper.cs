using System;
using System.Diagnostics;

namespace EarTrumpet.Interop.Helpers;

internal class SettingsPageHelper
{
    public static void Open(string page)
    {
        try
        {
            using (Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = $"ms-settings:{page}" }))
            { }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"SettingsPageHelper Open Failed: {ex}");
        }
    }
}
