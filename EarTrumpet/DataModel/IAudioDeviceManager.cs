using System;

namespace EarTrumpet.DataModel
{
    interface IAudioDeviceManager
    {
        event EventHandler<IAudioDevice> DefaultChanged;
        event EventHandler Loaded;

        IAudioDevice Default { get; set; }
        IAudioDeviceCollection Devices { get; }

        void MoveHiddenAppsToDevice(string appId, string id);
    }
}