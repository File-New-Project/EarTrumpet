using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    interface IAudioDeviceManager
    {
        event EventHandler<IAudioDevice> DefaultPlaybackDeviceChanged;
        event EventHandler PlaybackDevicesLoaded;

        IAudioDevice DefaultPlaybackDevice { get; set; }
        IAudioDevice DefaultCommunicationDevice { get; set; }
        ObservableCollection<IAudioDevice> Devices { get; }

        void MoveHiddenAppsToDevice(string appId, string id);
    }
}