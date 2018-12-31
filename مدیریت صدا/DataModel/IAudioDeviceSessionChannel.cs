using System.ComponentModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDeviceSessionChannel : INotifyPropertyChanged
    {
        float Level { get; set; }
    }
}