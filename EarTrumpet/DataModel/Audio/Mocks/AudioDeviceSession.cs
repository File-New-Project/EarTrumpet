using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.DataModel.WindowsAudio.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if DEBUG
namespace EarTrumpet.DataModel.Audio.Mocks
{
    class AudioDeviceSession : BindableBase, IAudioDeviceSessionInternal
    {
        public IEnumerable<IAudioDeviceSessionChannel> Channels => throw new NotImplementedException();

        public IAudioDevice Parent { get; }

        public string DisplayName { get; }

        public string ExeName => AppId;

        public uint BackgroundColor { get; }

        public string IconPath { get; }

        public bool IsDesktopApp { get; }

        public bool IsSystemSoundsSession { get; }

        public int ProcessId { get; }

        public string AppId { get; }

        public SessionState State { get; private set; }

        public ObservableCollection<IAudioDeviceSession> Children { get; } = new ObservableCollection<IAudioDeviceSession>();
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
            get => _volume;
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    RaisePropertyChanged(nameof(Volume));
                }
            }
        }

        public float PeakValue1 { get; set; }

        public float PeakValue2 { get; set; }

        public Guid GroupingParam => Guid.Empty;

        public AudioDeviceSession(IAudioDevice parent, string id, string displayName, string appId, string iconPath)
        {
            DisplayName = displayName;
            Id = id;
            AppId = appId;
            IconPath = iconPath;
            Parent = parent;
            IsDesktopApp = true;
        }

        public void Hide()
        {
 
        }

        public void UnHide()
        {
    
        }

        public void MoveToDevice(string id, bool hide)
        {
            throw new NotImplementedException();
        }

        public void UpdatePeakValueBackground()
        {

        }
    }
}
#endif
