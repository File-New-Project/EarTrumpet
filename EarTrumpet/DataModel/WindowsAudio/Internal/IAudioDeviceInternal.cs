namespace EarTrumpet.DataModel.WindowsAudio.Internal;

internal interface IAudioDeviceInternal
{
    void UpdatePeakValue();
    void MoveHiddenAppsToDevice(string appId, string id);
    void UnhideSessionsForProcessId(uint processId);
}
