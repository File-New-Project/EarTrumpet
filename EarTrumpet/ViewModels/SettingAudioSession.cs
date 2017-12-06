using EarTrumpet.DataModel;
using EarTrumpet.DataModel.Com;
using EarTrumpet.Services;
using System;
using System.ComponentModel;
using System.Linq;

namespace EarTrumpet.ViewModels
{
    class SettingAudioSession : BindableBase, IAudioDeviceSession
    {
        SettingsService.DefaultApp _cachedData;
        IAudioDeviceSession _liveSession;

        public SettingAudioSession(SettingsService.DefaultApp app, IAudioDeviceManager manager)
        {
            _cachedData = app;

            foreach(var dev in manager.Devices)
            {
                _liveSession = dev.Sessions.FirstOrDefault(s => AppResolverService.GetAppIdForProcess((uint)s.ProcessId) == _cachedData.Id);
                if (_liveSession != null)
                {
                    _cachedData.DisplayName = DisplayName;
                    _cachedData.BackgroundColor = BackgroundColor;
                    _cachedData.IconPath = IconPath;
                    _cachedData.IsDesktopApp = IsDesktopApp;
                    break;
                }
            }
        }

        public SettingsService.DefaultApp Data => _cachedData;

        public uint BackgroundColor => _liveSession != null ? _liveSession.BackgroundColor : _cachedData.BackgroundColor;

        public Guid GroupingParam => Guid.Empty;

        public string IconPath => _liveSession != null ? _liveSession.IconPath : _cachedData.IconPath;

        public bool IsDesktopApp => _liveSession != null ? _liveSession.IsDesktopApp : _cachedData.IsDesktopApp;

        public bool IsHidden => _liveSession != null ? _liveSession.IsHidden : false;

        public bool IsSystemSoundsSession => _liveSession != null ? _liveSession.IsSystemSoundsSession : false;

        public int ProcessId => _liveSession != null ? _liveSession.ProcessId : 0;

        public AudioSessionState State => _liveSession != null ? _liveSession.State : AudioSessionState.Active;

        public string DisplayName => _liveSession != null ? _liveSession.DisplayName : _cachedData.DisplayName;

        public string Id => _cachedData.Id;

        public bool IsMuted
        {
            get => _cachedData.IsMuted;
            set
            {
                _cachedData.IsMuted = value;
                RaisePropertyChanged(nameof(IsMuted));
            }
        }
        public float Volume
        {
            get => _cachedData.Volume;
            set
            {
                _cachedData.Volume = value;
                RaisePropertyChanged(nameof(Volume));
            }
        }

        public float PeakValue => 0;
    }
}
