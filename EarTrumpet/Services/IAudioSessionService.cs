using EarTrumpet.Models;
using System;
using System.Collections.Generic;

namespace EarTrumpet.Services
{
    public interface IAudioSessionService
    {
        IEnumerable<EarTrumpetAudioSessionModel> GetAudioSessions();
        void SetAudioSessionMute(uint sessionId, bool isMuted);
        void SetAudioSessionVolume(uint sessionId, float volume);
        IEnumerable<EarTrumpetAudioSessionModelGroup> GetAudioSessionGroups();

        event EventHandler<DisplayNameChangedArgs> DisplayNameChanged;
        event EventHandler<GroupingChangedArgs> GroupingChanged;
        event EventHandler<IconChangedArgs> IconChanged;
        event EventHandler<SessionStateChangedArgs> SessionStateChanged;
        event EventHandler<VolumeChangedArgs> VolumeChanged;
    }
}