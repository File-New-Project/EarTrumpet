using System;

namespace EarTrumpet.DataModel.Internal
{
    interface IAudioDeviceSessionInternal : IAudioDeviceSession
    {
        Guid GroupingParam { get; }
        void Hide();
        void UnHide();
        void MoveToDevice(string id, bool hide);
        void UpdatePeakValueBackground();
    }
}
