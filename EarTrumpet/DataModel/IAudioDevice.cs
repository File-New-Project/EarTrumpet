using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    interface IAudioDevice : IStreamWithVolumeControl
    {
        string DisplayName { get; }

        ObservableCollection<IAudioDeviceSession> Groups { get; }

        void UpdatePeakValueBackground();
        void UnhideSessionsForProcessId(int processId);
    }
}