using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    interface IAudioDeviceManager
    {
        event EventHandler<IAudioDevice> DefaultChanged;
        event EventHandler Loaded;

        IAudioDevice Default { get; set; }
        ObservableCollection<IAudioDevice> Devices { get; }

        void MoveHiddenAppsToDevice(string appId, string id);
    }
}