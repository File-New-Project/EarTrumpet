using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.DataModel.WindowsAudio.Internal;
using EarTrumpet.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if DEBUG
namespace EarTrumpet.DataModel.Audio.Mocks;

internal class AudioDevice : BindableBase, IAudioDevice, IAudioDeviceInternal, IAudioDeviceWindowsAudio
{
    public AudioDevice(string id, IAudioDeviceManager parent)
    {
        Parent = parent;
        Id = id;
    }

    public string DisplayName => Id;

    public string IconPath => null;

    public IAudioDeviceManager Parent { get; }

    public ObservableCollection<IAudioDeviceSession> Groups { get; } = [];

    public string Id { get; }

    private bool _isMuted;
    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            if (_isMuted != value)
            {
                _isMuted = value;
                RaisePropertyChanged(nameof(IsMuted));
            }
        }
    }
    private float _volume = 1;
    public float Volume
    {
        get
        {
            return App.Settings.UseLogarithmicVolume ? _volume.ToDisplayVolume() : _volume;
        }

        set
        {
            if (App.Settings.UseLogarithmicVolume)
            {
                value = value.ToLogVolume();
            }

            if (_volume != value)
            {
                _volume = value;
                RaisePropertyChanged(nameof(Volume));
            }
        }
    }

    public float PeakValue1 { get; set; }
    public float PeakValue2 { get; set; }
    public IEnumerable<IAudioDeviceChannel> Channels { get; } = new List<IAudioDeviceChannel>();
    public string EnumeratorName => "Mock Enumerator";
    public string InterfaceName => "Mock Interface";
    public string DeviceDescription => "Mock Description";

    public void AddFilter(Func<ObservableCollection<IAudioDeviceSession>, ObservableCollection<IAudioDeviceSession>> filter)
    {
    }

    public void UpdatePeakValue()
    {
    }

    public void MoveHiddenAppsToDevice(string appId, string id)
    {
    }

    public void UnhideSessionsForProcessId(uint processId)
    {
    }
}
#endif