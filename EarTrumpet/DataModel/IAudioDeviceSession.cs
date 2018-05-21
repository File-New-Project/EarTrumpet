using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    interface IAudioDeviceSession : IStreamWithVolumeControl
    {
        string DisplayName { get; }
        string ExeName { get; }
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