using System;
using System.ComponentModel;
using SoundControlAPI_Interop;

namespace EarTrumpet.DataModel
{
    public interface IAudioDeviceSession : INotifyPropertyChanged
    {
        uint BackgroundColor { get; }
        string DisplayName { get; }
        Guid GroupingParam { get; }
        string IconPath { get; }
        string Id { get; }
        bool IsDesktopApp { get; }
        bool IsHidden { get; }
        bool IsMuted { get; set; }
        bool IsSystemSoundsSession { get; }
        float PeakValue { get; }
        int ProcessId { get; }
        _AudioSessionState State { get; }
        float Volume { get; set; }
    }
}