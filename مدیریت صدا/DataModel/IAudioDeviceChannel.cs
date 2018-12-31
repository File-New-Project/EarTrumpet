using System.ComponentModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDeviceChannel : INotifyPropertyChanged
    {
        float Level { get; set; }
    }
}