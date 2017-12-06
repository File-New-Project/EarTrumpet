using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDeviceManager
    {
        IAudioDevice DefaultPlaybackDevice { get; set; }
        IAudioDevice DefaultCommunicationDevice { get; set; }
        ObservableCollection<IAudioDevice> Devices { get; }
        IVirtualDefaultAudioDevice VirtualDefaultDevice { get; }

        event EventHandler<IAudioDevice> DefaultPlaybackDeviceChanged;
        event EventHandler<IAudioDeviceSession> SessionCreated;
    }
}