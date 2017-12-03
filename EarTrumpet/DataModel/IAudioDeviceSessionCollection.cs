using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDeviceSessionCollection
    {
        ObservableCollection<IAudioDeviceSession> Sessions { get; }
    }
}