using System.ComponentModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDevice : INotifyPropertyChanged
    {
        string DisplayName { get; }
        string Id { get; }
        bool IsMuted { get; set; }
        AudioDeviceSessionCollection Sessions { get; }
        float Volume { get; set; }
        float PeakValue { get; }
    }
}