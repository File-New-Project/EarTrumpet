using System.ComponentModel;

namespace EarTrumpet.DataModel.WindowsAudio
{
    public interface IAudioDeviceSessionChannel : INotifyPropertyChanged
    {
        float Level { get; set; }
    }
}