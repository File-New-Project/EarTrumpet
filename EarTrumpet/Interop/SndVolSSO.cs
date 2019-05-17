using EarTrumpet.Interop.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;

namespace EarTrumpet.Interop
{
    public class SndVolSSO
    {
        public enum IconId
        {
            Muted = 120,
            SpeakerZeroBars = 121,
            SpeakerOneBar = 122,
            SpeakerTwoBars = 123,
            SpeakerThreeBars = 124,
            NoDevice = 125,
        }

        private static readonly string DllPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\SndVolSSO.dll");

        public static string GetPath(IconId icon)
        {
            return $"{DllPath},{(int)icon}";
        }

        public static bool SystemIconsAreAvailable()
        {
            Func<IconId, Icon> ThrowIfNull = (icon) => IconHelper.LoadSmallIcon(GetPath(icon)) ?? throw new InvalidOperationException(icon.ToString());

            try
            {
                ThrowIfNull(IconId.Muted);
                ThrowIfNull(IconId.NoDevice);
                ThrowIfNull(IconId.SpeakerOneBar);
                ThrowIfNull(IconId.SpeakerTwoBars);
                ThrowIfNull(IconId.SpeakerThreeBars);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SndVolSSO VerifySystemIconsAreAvailable Failed: {ex}");
                return false;
            }
        }
    }
}
