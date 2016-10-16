using EarTrumpet.Models;
using System;
using System.Collections.Generic;

namespace EarTrumpet.Services
{
    public interface IAudioSessionService
    {
        IEnumerable<AudioSessionModel> GetAudioSessions();
        void SetAudioSessionMute(uint sessionId, bool isMuted);
        void SetAudioSessionVolume(uint sessionId, float volume);
        IEnumerable<EarTrumpetAudioSessionModelGroup> GetAudioSessionGroups();
    }
}