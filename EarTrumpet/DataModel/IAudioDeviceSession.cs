using EarTrumpet.DataModel.Com;
using System;

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
        AudioSessionState State { get; }

        string ActiveOnOtherDevice { get; set; }
    }
}