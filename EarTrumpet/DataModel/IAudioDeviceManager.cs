using EarTrumpet.Interop.MMDeviceAPI;
using System;

namespace EarTrumpet.DataModel
{
    public interface IAudioDeviceManager
    {
        event EventHandler<IAudioDevice> DefaultChanged;
        event EventHandler Loaded;

        IAudioDevice Default { get; set; }
        IAudioDeviceCollection Devices { get; }
        AudioDeviceKind DeviceKind { get; }

        void MoveHiddenAppsToDevice(string appId, string id);
        void SetDefaultEndPoint(string id, int pid);
        string GetDefaultEndPoint(int processId);
    }

    public interface IAudioDeviceManagerInternal
    {
        void SetDefaultDevice(IAudioDevice device, ERole role = ERole.eMultimedia);
    }
}