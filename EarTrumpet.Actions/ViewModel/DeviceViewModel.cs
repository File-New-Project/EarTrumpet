using EarTrumpet.DataModel;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Serialization;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet_Actions.ViewModel
{
    class DeviceViewModel : BindableBase, IOptionViewModel
    {
        [Flags]
        public enum DeviceListKind
        {
            Playback = 0,
            Recording = 1,
            DefaultPlayback = 2
        }

        public ObservableCollection<Option> All { get; }

        public Option Selected
        {
            get => All.FirstOrDefault(d => ((Device)d.Value)?.Id == _part.Device?.Id);
            set
            {
                if (Selected != value)
                {
                    _part.Device = (Device)value.Value;
                    RaisePropertyChanged(nameof(Selected));
                }
            }
        }

        private IPartWithDevice _part;

        public DeviceViewModel(IPartWithDevice part, DeviceListKind flags)
        {
            _part = part;
            All = GetDevices(flags);
            if (Selected == null && _part.Device?.Id != null)
            {
                All.Add(new Option(_part.Device.Id, _part.Device));
            }

            if (_part.Device?.Id == null)
            {
                _part.Device = (Device)All[0].Value;
            }
        }

        public override string ToString()
        {
            return Selected?.DisplayName;
        }

        ObservableCollection<Option> GetDevices(DeviceListKind flags)
        {
            var ret = new ObservableCollection<Option>();

            bool isRecording = (flags & DeviceListKind.Recording) == DeviceListKind.Recording;

            if ((flags & DeviceListKind.DefaultPlayback) == DeviceListKind.DefaultPlayback)
            {
                ret.Add(new Option(Properties.Resources.DefaultPlaybackDeviceText, new Device { Kind = AudioDeviceKind.Playback }));
            }

            foreach (var device in DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback).Devices)
            {
                ret.Add(new Option(isRecording ? string.Format(Properties.Resources.PlaybackDeviceFormatStringText, device.DisplayName) : device.DisplayName, CreateDevice(device)));
            }

            if (isRecording)
            {
                foreach (var device in DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Recording).Devices)
                {
                    ret.Add(new Option(string.Format(Properties.Resources.RecordingDeviceFormatStringText, device.DisplayName), CreateDevice(device)));
                }
            }
            return ret;
        }

        Device CreateDevice(IAudioDevice device)
        {
            return new Device
            {
                Id = device.Id,
                Kind = device.Parent.DeviceKind,
            };
        }
    }
}