using System;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public enum ChangeDeviceVolumeActionKind
    {
        Mute,
        Unmute,
        SetVolume,
        ToggleMute,
    }

    public class ChangeDeviceVolumeAction : BaseAction, IPartWithDevice, IPartWithVolume
    {
        public Device Device { get; set; }
        public ChangeDeviceVolumeActionKind Operation { get; set; }
        public double Volume { get; set; }

        public ChangeDeviceVolumeAction()
        {
            Description = "Change a device volume or mute";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                new Option("mute", ChangeDeviceVolumeActionKind.Mute),
                new Option("set volume", ChangeDeviceVolumeActionKind.SetVolume),
                new Option("toggle mute", ChangeDeviceVolumeActionKind.ToggleMute),
                new Option("unmute", ChangeDeviceVolumeActionKind.Unmute),
                },
                (newValue) => Operation = (ChangeDeviceVolumeActionKind)newValue.Value,
                () => Operation) });
        }

        public override void Invoke()
        {
            var device = PlaybackDataModelHost.DeviceManager.Devices.FirstOrDefault(d => d.Id == Device.Id);
            if (device != null)
            {
                switch (Operation)
                {
                    case ChangeDeviceVolumeActionKind.Mute:
                        device.IsMuted = true;
                        break;
                    case ChangeDeviceVolumeActionKind.ToggleMute:
                        device.IsMuted = !device.IsMuted;
                        break;
                    case ChangeDeviceVolumeActionKind.Unmute:
                        device.IsMuted = false;
                        break;
                    case ChangeDeviceVolumeActionKind.SetVolume:
                        device.Volume = (float)(Volume / 100f);
                        break;
                }
            }
        }

        public override string Describe()
        {
            if (Operation == ChangeDeviceVolumeActionKind.SetVolume)
            {
                return $"Set volume to {Math.Round(Volume)}% on {Device}";
            }
            else
            {
                return $"Set {Options[0].DisplayName} on {Device}";
            }
        }
    }
}
