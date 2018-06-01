using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    interface IAudioDeviceManager
    {
        IAudioDevice DefaultPlaybackDevice { get; set; }
        IAudioDevice DefaultCommunicationDevice { get; set; }
        ObservableCollection<IAudioDevice> Devices { get; }
        IVirtualDefaultAudioDevice VirtualDefaultDevice { get; }

        event EventHandler<IAudioDevice> DefaultPlaybackDeviceChanged;
    }
}