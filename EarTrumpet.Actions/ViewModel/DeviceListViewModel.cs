using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Serialization;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace EarTrumpet_Actions.ViewModel
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

            if (ToString() == null)
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

            foreach (var device in DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback).Devices)
            {
                All.Add(new DeviceViewModel(device));
            }

            if (isRecording)
            {
                foreach (var device in DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Recording).Devices)
                {
                    All.Add(new DeviceViewModel(device));
                }
            }
        }
    }
}