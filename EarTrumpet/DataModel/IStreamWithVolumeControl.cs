using System.ComponentModel;

namespace EarTrumpet.DataModel
{
    interface IStreamWithVolumeControl : INotifyPropertyChanged
    {
        string Id { get; }
        bool IsMuted { get; set; }
        float Volume { get; set; }
        float PeakValue { get; }
    }
}
