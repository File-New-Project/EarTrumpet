using EarTrumpet.DataModel.WindowsAudio;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel.Audio
{
    public interface IAudioDeviceSession : IStreamWithVolumeControl
    {
        IEnumerable<IAudioDeviceSessionChannel> Channels { get; }
        IAudioDevice Parent { get; }
        string DisplayName { get; }
        string ExeName { get; }
        string IconPath { get; }
        bool IsDesktopApp { get; }
        bool IsSystemSoundsSession { get; }
        uint ProcessId { get; }
        string AppId { get; }
        SessionState State { get; }
        ObservableCollection<IAudioDeviceSession> Children { get; }
    }
}