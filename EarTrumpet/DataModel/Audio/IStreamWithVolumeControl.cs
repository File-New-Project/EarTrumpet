using System.ComponentModel;

namespace EarTrumpet.DataModel.Audio;

public interface IStreamWithVolumeControl : INotifyPropertyChanged
{
    string Id { get; }
    bool IsMuted { get; set; }
    float Volume { get; set; }
    float PeakValue1 { get; }
    float PeakValue2 { get; }

    // Unlike Volume property, these methods are independent of app unit settings.
    float GetVolumeScalar();
    float GetVolumeLogarithmic();
    void SetVolumeScalar(float value);
    void SetVolumeLogarithmic(float value);
}
