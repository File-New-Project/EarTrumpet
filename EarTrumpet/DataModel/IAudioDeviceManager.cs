using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDeviceManager
    {
        event EventHandler<IAudioDevice> DefaultChanged;
        event EventHandler Loaded;

        IAudioDevice Default { get; }
        ObservableCollection<IAudioDevice> Devices { get; }
        AudioDeviceKind DeviceKind { get; }
        void SetDefaultDevice(IAudioDevice device, ERole role = ERole.eMultimedia);
        IAudioDevice GetDefaultDevice(ERole role = ERole.eMultimedia);
        void MoveHiddenAppsToDevice(string appId, string id);
        void SetDefaultEndPoint(string id, int pid);
        string GetDefaultEndPoint(int processId);
        void AddFilter(Func<ObservableCollection<IAudioDevice>, ObservableCollection<IAudioDevice>> filter);
    }
}