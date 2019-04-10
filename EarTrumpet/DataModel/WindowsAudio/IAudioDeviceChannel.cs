using System.ComponentModel;

namespace EarTrumpet.DataModel.WindowsAudio
{
    public interface IAudioDeviceChannel : INotifyPropertyChanged
    {
        float Level { get; set; }
    }
}