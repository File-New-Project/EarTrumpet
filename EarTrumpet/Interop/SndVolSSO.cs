using System;

namespace EarTrumpet.Interop
{
    public class SndVolSSO
    {
        public static readonly string DllPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\SndVolSSO.dll");

        public enum IconId
        {
            Invalid = 0,
            Muted = 120,
            SpeakerZeroBars = 121,
            SpeakerOneBar = 122,
            SpeakerTwoBars = 123,
            SpeakerThreeBars = 124,
            NoDevice = 125,
            OriginalIcon
        }
    }
}
