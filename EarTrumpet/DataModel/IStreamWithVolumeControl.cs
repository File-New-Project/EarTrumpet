using System.ComponentModel;

namespace EarTrumpet.DataModel
{
    public interface IStreamWithVolumeControl : INotifyPropertyChanged
    {
        string Id { get; }
        bool IsMuted { get; set; }
        float Volume { get; set; }
        float PeakValue1 { get; }
        float PeakValue2 { get; }

        bool UseLogarithmicVolume { get; set; }
    }
}
