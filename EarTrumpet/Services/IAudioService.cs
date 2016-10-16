using System;
using System.Collections.Generic;
using EarTrumpet.Models;

namespace EarTrumpet.Services
{
    public interface IAudioService
    {
        event EventHandler<DeviceVolumeChangedArgs> DeviceVolumeChanged;
        event EventHandler<SessionDisplayNameChangedArgs> SessionDisplayNameChanged;
        event EventHandler<SessionGroupingChangedArgs> SessionGroupingChanged;
        event EventHandler<SessionIconChangedArgs> SessionIconChanged;
        event EventHandler<SessionStateChangedArgs> SessionStateChanged;
        event EventHandler<SessionVolumeChangedArgs> SessionVolumeChanged;

        IEnumerable<AudioDeviceAndSessionsModel> GetAudioDevicesAndSessions();
    }
}