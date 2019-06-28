using System;

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
    }
}
