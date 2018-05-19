using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDevice : IStreamWithVolumeControl
    {
        string DisplayName { get; }

        ObservableCollection<IAudioDeviceSession> Groups { get; }
    }
}