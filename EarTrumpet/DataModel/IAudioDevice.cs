﻿using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    interface IAudioDevice : IStreamWithVolumeControl
    {
        string DisplayName { get; }
        IAudioDeviceManager Parent { get; }

        ObservableCollection<IAudioDeviceSession> Groups { get; }

        void UpdatePeakValueBackground();
        void UnhideSessionsForProcessId(int processId);
        void MoveHiddenAppsToDevice(string appId, string id);
    }
}