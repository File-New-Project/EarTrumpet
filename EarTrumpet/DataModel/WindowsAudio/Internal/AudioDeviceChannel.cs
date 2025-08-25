using System;
using System.ComponentModel;
using Windows.Win32.Media.Audio.Endpoints;

namespace EarTrumpet.DataModel.WindowsAudio.Internal;

internal class AudioDeviceChannel : BindableBase, INotifyPropertyChanged, IAudioDeviceChannel
{
    private float _level;
    private uint _index;
    private IAudioEndpointVolume _deviceVolume;

    public AudioDeviceChannel(IAudioEndpointVolume deviceVolume, uint index)
    {
        _index = index;
        _deviceVolume = deviceVolume;
        if (App.Settings.UseLogarithmicVolume)
        {
            _deviceVolume.GetChannelVolumeLevel(index, out _level);
        }
        else
        {
            _deviceVolume.GetChannelVolumeLevelScalar(index, out _level);
        }
    }

    public float Level
    {
        get => _level;
        set
        {
            if (_level != value)
            {
                var context = Guid.Empty;
                unsafe
                {
                    if (App.Settings.UseLogarithmicVolume)
                    {
                        _deviceVolume.SetChannelVolumeLevel(_index, value, &context);
                    }
                    else
                    {
                        _deviceVolume.SetChannelVolumeLevelScalar(_index, value, &context);
                    }
                }

                _level = value;
                RaisePropertyChanged(nameof(Level));
            }
        }
    }

    internal void OnNotify(float newLevel)
    {
        if (newLevel != _level)
        {
            _level = newLevel;
            RaisePropertyChanged(nameof(Level));
        }
    }
}
