using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    interface IAudioDeviceSession : IStreamWithVolumeControl
    {
        string DisplayName { get; }
        string ExeName { get; }
        uint BackgroundColor { get; }
        Guid GroupingParam { get; }
        string IconPath { get; }
        bool IsDesktopApp { get; }
        bool IsSystemSoundsSession { get; }
        int ProcessId { get; }
        string AppId { get; }
        SessionState State { get; }
        ObservableCollection<IAudioDeviceSession> Children { get; }
        void MoveFromDevice();
    }
}