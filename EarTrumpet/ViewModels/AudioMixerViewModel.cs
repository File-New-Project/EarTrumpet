using EarTrumpet.Extensions;
using EarTrumpet.Models;
using EarTrumpet.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System;

namespace EarTrumpet.ViewModels
{
    public class AudioMixerViewModel : BindableBase
    {
        // This encapsulates the functionality used for AppItemViewModels to call back to AudioMixerViewModel
        // and thus interact with the audio session service.
        public class AudioMixerViewModelCallbackProxy : IAudioMixerViewModelCallback
        {
            private readonly EarTrumpetAudioSessionService _service;   
            private readonly EarTrumpetAudioDeviceService _deviceService;   
            public AudioMixerViewModelCallbackProxy(EarTrumpetAudioSessionService service, EarTrumpetAudioDeviceService deviceService)
            {
                _service = service;
                _deviceService = deviceService;
            }

            // IAudioMixerViewModelCallback
            public void SetVolume(EarTrumpetAudioSessionModel item, float volume)
            {
                _service.SetAudioSessionVolume(item.SessionId, volume);
            }

            public void SetMute(EarTrumpetAudioSessionModel item, bool isMuted)
            {
                _service.SetAudioSessionMute(item.SessionId, isMuted);
            }

            public void SetDeviceVolume(EarTrumpetAudioDeviceModel device, float volume)
            {
                _deviceService.SetAudioDeviceVolume(device.Id, volume);
            }

            public void SetDeviceMute(EarTrumpetAudioDeviceModel device, bool isMuted)
            {
                if (isMuted)
                {
                    _deviceService.MuteAudioDevice(device.Id);
                }
                else
                {
                    _deviceService.UnmuteAudioDevice(device.Id);
                }
            }
        }

        public ObservableCollection<AppItemViewModel> Apps { get; private set; }
        public DeviceAppItemViewModel Device { get; private set; }

        public Visibility ListVisibility { get; private set; }
        public Visibility NoAppsPaneVisibility { get; private set; }
        public Visibility DeviceVisibility { get; private set; }

        public string NoItemsContent { get; private set; }

        private readonly EarTrumpetAudioSessionService _audioService;
        private readonly EarTrumpetAudioDeviceService _deviceService;
        private readonly AudioMixerViewModelCallbackProxy _proxy;

        public AudioMixerViewModel()
        {
            Apps = new ObservableCollection<AppItemViewModel>();
            _audioService = new EarTrumpetAudioSessionService();
            _deviceService = new EarTrumpetAudioDeviceService();
            _proxy = new AudioMixerViewModelCallbackProxy(_audioService, _deviceService);
        }

        public void Refresh()
        {
            var devices = _deviceService.GetAudioDevices();
            if (devices.Any())
            {
                var defaultDevice = devices.FirstOrDefault(x => x.IsDefault);
                var volume = _deviceService.GetAudioDeviceVolume(defaultDevice.Id);
                var newDevice = new DeviceAppItemViewModel(_proxy, defaultDevice, volume);
                if (Device != null && Device.IsSame(newDevice))
                {
                    Device.UpdateFromOther(newDevice);
                }
                else
                {
                    Device = newDevice;
                }
                RaisePropertyChanged("Device");
            }
            
            bool hasApps = Apps.Count > 0;
            var sessions = _audioService.GetAudioSessionGroups().Select(x => new AppItemViewModel(_proxy, x));

            List<AppItemViewModel> staleSessionsToRemove = new List<AppItemViewModel>();
            
            // remove stale apps
            foreach (var app in Apps)
            {
                if (!sessions.Where(x => (x.IsSame(app))).Any())
                {
                    staleSessionsToRemove.Add(app);
                }
            }
            foreach (var app in staleSessionsToRemove) { Apps.Remove(app); }

            // add new apps
            foreach (var session in sessions)
            {
                var findApp = Apps.FirstOrDefault(x => x.IsSame(session));
                if (findApp == null)
                {
                    Apps.AddSorted(session, AppItemViewModelComparer.Instance);
                }
                else
                {
                    // update existing apps
                    findApp.UpdateFromOther(session);
                }
            }
            
            ListVisibility = Apps.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            NoAppsPaneVisibility = Apps.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            NoItemsContent = Device == null ? Properties.Resources.NoDevicesPanelContent : Properties.Resources.NoAppsPanelContent;
            DeviceVisibility = Device != null ? Visibility.Visible : Visibility.Collapsed;

            RaisePropertyChanged("ListVisibility");
            RaisePropertyChanged("NoAppsPaneVisibility");
            RaisePropertyChanged("NoItemsContent");
            RaisePropertyChanged("DeviceVisibility");
        }
    }
}
