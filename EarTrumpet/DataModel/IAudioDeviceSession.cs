using Interop.SoundControlAPI;
using System;

namespace EarTrumpet.DataModel
{
    public interface IAudioDeviceSession : IStreamWithVolumeControl
    {
        uint BackgroundColor { get; }
        Guid GroupingParam { get; }
        string IconPath { get; }
        bool IsDesktopApp { get; }
        bool IsHidden { get; }
        bool IsSystemSoundsSession { get; }
        int ProcessId { get; }
        _AudioSessionState State { get; }
    }
}