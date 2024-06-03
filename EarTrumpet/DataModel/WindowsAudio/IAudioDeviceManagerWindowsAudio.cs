using EarTrumpet.DataModel.Audio;
using Windows.Win32.Media.Audio;

namespace EarTrumpet.DataModel.WindowsAudio;

public interface IAudioDeviceManagerWindowsAudio
{
    void SetDefaultDevice(IAudioDevice device, ERole role);
    IAudioDevice GetDefaultDevice(ERole role);
    void SetDefaultEndPoint(string id, uint processId);
    string GetDefaultEndPoint(uint processId);
    void MoveHiddenAppsToDevice(string appId, string deviceId);
    void UnhideSessionsForProcessId(string deviceId, uint processId);
}
