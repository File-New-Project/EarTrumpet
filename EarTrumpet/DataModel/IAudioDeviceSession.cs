using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDeviceSession : IStreamWithVolumeControl
    {
        IAudioDevice Parent { get; }
        string SessionDisplayName { get; }
        void RefreshDisplayName();
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
        string PersistedDefaultEndPointId { get; }
        void Hide();
        void UnHide();
        void MoveToDevice(string id, bool hide);
        void UpdatePeakValueBackground();
    }
}