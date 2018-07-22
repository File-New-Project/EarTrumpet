using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDevice : IStreamWithVolumeControl
    {
        string DisplayName { get; }
        IAudioDeviceManager Parent { get; }

        ObservableCollection<IAudioDeviceSession> Groups { get; }

        void UpdatePeakValueBackground();
        void UnhideSessionsForProcessId(int processId);
        void MoveHiddenAppsToDevice(string appId, string id);
    }

    public interface IAudioDeviceInternal
    {
        void AddSessionFilter(Func<ObservableCollection<IAudioDeviceSession>, ObservableCollection<IAudioDeviceSession>> filter);
    }
}