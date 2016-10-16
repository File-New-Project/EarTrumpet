using System.Collections.Generic;
using EarTrumpet.Models;

namespace EarTrumpet.Services
{
    public interface IAudioDeviceService
    {
        IEnumerable<AudioDeviceModel> GetAudioDevices();
        float GetAudioDeviceVolume(string deviceId);
        void MuteAudioDevice(string deviceId);
        void SetAudioDeviceVolume(string deviceId, float volume);
        void SetDefaultAudioDevice(string deviceId);
        void UnmuteAudioDevice(string deviceId);
    }
}