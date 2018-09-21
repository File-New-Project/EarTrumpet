using EarTrumpet.DataModel.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDevice : IStreamWithVolumeControl
    {
        IEnumerable<IAudioDeviceChannel> Channels { get; }
        string DisplayName { get; }
        string IconPath { get; }
        string EnumeratorName { get; }
        string InterfaceName { get; }
        string DeviceDescription { get; }
        IAudioDeviceManager Parent { get; }
        ObservableCollection<IAudioDeviceSession> Groups { get; }
        void UpdatePeakValueBackground();
        void UnhideSessionsForProcessId(int processId);
        void MoveHiddenAppsToDevice(string appId, string id);
        void AddSessionFilter(Func<ObservableCollection<IAudioDeviceSession>, ObservableCollection<IAudioDeviceSession>> filter);
    }
}