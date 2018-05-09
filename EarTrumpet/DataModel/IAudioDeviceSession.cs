using EarTrumpet.DataModel.Com;
using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDeviceSession : IStreamWithVolumeControl
    {
        IAudioDevice Device { get; }
        uint BackgroundColor { get; }
        Guid GroupingParam { get; }
        string IconPath { get; }
        bool IsDesktopApp { get; }
        bool IsSystemSoundsSession { get; }
        int ProcessId { get; }
        string AppId { get; }
        AudioSessionState State { get; }
        ObservableCollection<IAudioDeviceSession> Children { get; }
    }
}