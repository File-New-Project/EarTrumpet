using EarTrumpet.DataModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet_Actions.DataModel
{
    public class Device
    {
        [Flags]
        public enum DeviceListKind
        {
            Playback = 0,
            Recording = 1,
            DefaultPlayback = 2
        }

        public static ObservableCollection<Option> GetDevices(DeviceListKind flags)
        {
            var ret = new ObservableCollection<Option>();

            bool isRecording = (flags & DeviceListKind.Recording) == DeviceListKind.Recording;

            if ((flags & DeviceListKind.DefaultPlayback) == DeviceListKind.DefaultPlayback)
            {
                ret.Add(new Option("Default playback device", new Device { Kind = AudioDeviceKind.Playback }));
            }

            foreach (var device in DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback).Devices)
            {
                ret.Add(new Option(isRecording ? $"Playback: {device.DisplayName}" : device.DisplayName, new Device(device)));
            }

            if (isRecording)
            {
                foreach (var device in DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Recording).Devices)
                {
                    ret.Add(new Option($"Recording: {device.DisplayName}", new Device(device)));
                }
            }
            return ret;
        }

        public Device()
        {

        }

        public Device(IAudioDevice device)
        {
            Id = device.Id;
            Kind = device.Parent.DeviceKind;
        }

        public string Id { get; set; }

        public AudioDeviceKind Kind { get; set; }

        public override string ToString()
        {
            if (Id == null)
            {
                return "Default playback device";
            }

            var device = DataModelFactory.CreateAudioDeviceManager(Kind).Devices.FirstOrDefault(d => d.Id == Id);
            if (device != null)
            {
                return device.DisplayName;
            }
            return Id;
        }

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.GetHashCode();
        }
    }
}
