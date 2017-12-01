using EarTrumpet.DataModel.Interfaces;
using SoundControlAPI_Interop;

namespace EarTrumpet.Extensions
{
    public static class IAudioSessionControlExtensions
    {
        static readonly int AUDCLNT_SESSIONFLAGS_DISPLAY_HIDE = 0x20000000;

        public static bool IsHidden(this IAudioSessionControl obj)
        {
            ((IAudioSessionControlInternal)obj).GetStreamFlags(out int flags);
            return (flags & AUDCLNT_SESSIONFLAGS_DISPLAY_HIDE) == AUDCLNT_SESSIONFLAGS_DISPLAY_HIDE;
        }
    }
}
