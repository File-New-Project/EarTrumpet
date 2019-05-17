using System;
using System.Diagnostics;
using System.IO;

namespace EarTrumpet.Interop.Helpers
{
    class LegacyControlPanelHelper
    {
        public static void Open(string panel)
        {
            try
            {
                var rundllPath = Path.Combine(
                    Environment.GetEnvironmentVariable("SystemRoot"),
                    (Environment.Is64BitOperatingSystem ? @"sysnative\rundll32.exe" : @"system32\rundll32.exe"));

                using (Process.Start(rundllPath, $"shell32.dll,Control_RunDLL mmsys.cpl,,{panel}"))
                { }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"LegacyControlPanelHelper Open Failed: {ex}");
            }
        }

        public static void StartLegacyAudioMixer()
        {
            try
            {
                var pos = System.Windows.Forms.Cursor.Position;
                using (Process.Start("sndvol.exe", $"-a {User32.MAKEWPARAM((ushort)pos.X, (ushort)pos.Y)}"))
                { }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"LegacyControlPanelHelper StartLegacyAudioMixer Failed: {ex}");
            }
        }
    }
}
