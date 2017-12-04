using Interop.SoundControlAPI;
using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Extensions
{
    public static class IAudioSessionControl2Extensons
    {
        // Duplicate due to IsSystemSoundsSession needing PreserveSig to read S_OK/S_FALSE.
        [Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioSessionControl2_Shim
        {
            void GetState(out _AudioSessionState pRetVal);
            void GetDisplayName(out string pRetVal);
            void SetDisplayName(string Value, ref Guid EventContext);
            void GetIconPath(out string pRetVal);
            void SetIconPath(string Value, ref Guid EventContext);
            void GetGroupingParam(out Guid pRetVal);
            void SetGroupingParam(ref Guid Override, ref Guid EventContext);
            void RegisterAudioSessionNotification(IAudioSessionEvents NewNotifications);
            void UnregisterAudioSessionNotification(IAudioSessionEvents NewNotifications);
            void GetSessionIdentifier(out string pRetVal);
            void GetSessionInstanceIdentifier(out string pRetVal);
            void GetProcessId(out uint pRetVal);
            [PreserveSig]
            int IsSystemSoundsSession();
            void SetDuckingPreference(int optOut);
        }

        public static bool GetIsSystemSoundsSession(this IAudioSessionControl2 obj)
        {
            return ((IAudioSessionControl2_Shim)obj).IsSystemSoundsSession() == 0;
        }
    }
}
