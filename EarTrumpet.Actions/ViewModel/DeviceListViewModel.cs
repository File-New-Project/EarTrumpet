using EarTrumpet.Extensions;
using EarTrumpet.Actions.DataModel;
using EarTrumpet.Actions.DataModel.Serialization;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using EarTrumpet.DataModel.WindowsAudio;

namespace EarTrumpet.Actions.ViewModel
{
    public class DeviceListViewModel : BindableBase
    {
        [Flags]
        public enum DeviceListKind
        {
            Playback = 0,
            Recording = 1,
            DefaultPlayback = 2
        }

        public ObservableCollection<DeviceViewModelBase> All { get; }

        public void OnInvoked(object sender, DeviceViewModelBase vivewModel)
        {
            _part.Device = new Device { Id = vivewModel.Id, Kind = vivewModel.Kind };
            RaisePropertyChanged("");  // Signal change so ToString will be called.

            var popup = ((DependencyObject)sender).FindVisualParent<Popup>();
            popup.IsOpen = false;
        }

        private IPartWithDevice _part;

        public DeviceListViewModel(IPartWithDevice part, DeviceListKind flags)
        {
            _part = part;
            All = new ObservableCollection<DeviceViewModelBase>();
            GetDevices(flags);

            if (_part.Device == null)
            {
                _part.Device = new Device { Id = All[0].Id, Kind = All[0].Kind };
            }
        }

        public override string ToString()
        {
            var existing = All.FirstOrDefault(d => d.Id == _part.Device?.Id);
            if (existing != null)
            {
                return existing.DisplayName;
            }
            return _part.Device?.Id;
        }

        void GetDevices(DeviceListKind flags)
        {
            bool isRecording = (flags & DeviceListKind.Recording) == DeviceListKind.Recording;

            if ((flags & DeviceListKind.DefaultPlayback) == DeviceListKind.DefaultPlayback)
            {
                All.Add(new DefaultPlaybackDeviceViewModel());
            }

            foreach (var device in WindowsAudioFactory.Create(AudioDeviceKind.Playback).Devices.OrderBy(d => d.DisplayName))
            {
                All.Add(new DeviceViewModel(device));
            }

            if (isRecording)
            {
                foreach (var device in WindowsAudioFactory.Create(AudioDeviceKind.Recording).Devices.OrderBy(d => d.DisplayName))
                {
                    All.Add(new DeviceViewModel(device));
                }
            }
        }
    }
}