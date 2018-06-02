using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace EarTrumpet.ViewModels
{
    public class DeviceViewModel : AudioSessionViewModel
    {
        public string DisplayName => _device.DisplayName;
        public ObservableCollection<AppItemViewModel> Apps { get; private set; }
        public string DeviceIconText { get; private set; }
        public string DeviceIconTextBackground { get; private set; }

        private readonly string s_Sound3BarsIcon = "\xE995";
        private readonly string s_Sound2BarsIcon = "\xE994";
        private readonly string s_Sound1BarIcon = "\xE993";
        private readonly string s_SoundMuteIcon = "\xE74F";

        private IAudioDevice _device;
        private IAudioDeviceManager _deviceManager;

        internal DeviceViewModel(IAudioDeviceManager deviceManager, IAudioDevice device) : base(device)
        {
            _deviceManager = deviceManager;
            _device = device;

            Apps = new ObservableCollection<AppItemViewModel>();

            _device.PropertyChanged += Device_PropertyChanged;
            _device.Groups.CollectionChanged += Sessions_CollectionChanged;

            foreach (var session in _device.Groups)
            {
                Apps.AddSorted(new AppItemViewModel(session), AppItemViewModel.CompareByExeName);
            }

            UpdateMasterVolumeIcon();
        }

        ~DeviceViewModel()
        {
            _device.PropertyChanged -= Device_PropertyChanged;
            _device.Groups.CollectionChanged -= Sessions_CollectionChanged;
        }

        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_device.IsMuted) ||
                e.PropertyName == nameof(_device.Volume))
            {
                UpdateMasterVolumeIcon();
            }
            else if (e.PropertyName == nameof(_device.DisplayName))
            {
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        public override void TriggerPeakCheck()
        {
            base.TriggerPeakCheck();

            foreach (var app in Apps) app.TriggerPeakCheck();
        }

        private void UpdateMasterVolumeIcon()
        {
            string icon;
            if (_device.IsMuted)
            {
                icon = s_SoundMuteIcon;
            }
            else if (_device.Volume >= 0.65f)
            {
                icon = s_Sound3BarsIcon;
            }
            else if (_device.Volume >= 0.33f)
            {
                icon = s_Sound2BarsIcon;
            }
            else if (_device.Volume > 0f)
            {
                icon = s_Sound1BarIcon;
            }
            else
            {
                icon = s_SoundMuteIcon;
            }

            DeviceIconText = icon;
            DeviceIconTextBackground = (icon == s_SoundMuteIcon) ? s_SoundMuteIcon : s_Sound3BarsIcon;

            RaisePropertyChanged(nameof(DeviceIconText));
            RaisePropertyChanged(nameof(DeviceIconTextBackground));
        }

        private void Sessions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    AddSession((IAudioDeviceSession)e.NewItems[0]);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    var existing = Apps.FirstOrDefault(x => x.Id == ((IAudioDeviceSession)e.OldItems[0]).Id);
                    if (existing != null)
                    {
                        Apps.Remove(existing);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void AddSession(IAudioDeviceSession session)
        {
            var newSession = new AppItemViewModel(session);

            foreach(var a in Apps)
            {
                if (a.DoesGroupWith(newSession))
                {
                    // Remove the fake app entry.
                    Apps.Remove(a);
                    break;
                }
            }

            Apps.AddSorted(newSession, AppItemViewModel.CompareByExeName);
        }

        public void OnAppMovedToDevice(AppItemViewModel app)
        {
            bool hasExistingAppGroup = false;
            foreach(var a in Apps)
            {
                if (a.DoesGroupWith(app))
                {
                    hasExistingAppGroup = true;
                    break;
                }
            }

            if (!hasExistingAppGroup)
            {
                // Add a fake app entry.
                Apps.AddSorted(app, AppItemViewModel.CompareByExeName);
            }
        }

        public void MakeDefaultPlaybackDevice()
        {
            _deviceManager.DefaultPlaybackDevice = _device;
        }

        public override string ToString() => string.Format(IsMuted ? Properties.Resources.AppOrDeviceMutedFormatAccessibleText : Properties.Resources.AppOrDeviceFormatAccessibleText, DisplayName, Volume);
    }
}
