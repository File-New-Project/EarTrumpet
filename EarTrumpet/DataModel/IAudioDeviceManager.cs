using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDeviceManager
    {
        IAudioDevice DefaultDevice { get; set; }
        ObservableCollection<IAudioDevice> Devices { get; }
        IVirtualDefaultAudioDevice VirtualDefaultDevice { get; }

        event EventHandler<IAudioDevice> DefaultDeviceChanged;
        event EventHandler<IAudioDeviceSession> SessionCreated;
    }
}