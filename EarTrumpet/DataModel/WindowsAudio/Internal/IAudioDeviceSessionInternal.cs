using EarTrumpet.DataModel.Audio;
using System;

namespace EarTrumpet.DataModel.WindowsAudio.Internal;

internal interface IAudioDeviceSessionInternal : IAudioDeviceSession
{
    Guid GroupingParam { get; }
    void Hide();
    void UnHide();
    void MoveToDevice(string id, bool hide);
    void UpdatePeakValueBackground();
}
