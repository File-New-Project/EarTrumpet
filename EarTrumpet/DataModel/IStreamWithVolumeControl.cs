using System.ComponentModel;

namespace EarTrumpet.DataModel
{
    public interface IStreamWithVolumeControl : INotifyPropertyChanged
    {
        string DisplayName { get; }
        string Id { get; }
        bool IsMuted { get; set; }
        float Volume { get; set; }
        float PeakValue { get; }
    }
}
