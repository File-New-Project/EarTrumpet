using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDevice : IStreamWithVolumeControl
    {
        ObservableCollection<IAudioDeviceSession> Sessions { get; }

        void TakeSessionFromOtherDevice(int processId);
    }
}