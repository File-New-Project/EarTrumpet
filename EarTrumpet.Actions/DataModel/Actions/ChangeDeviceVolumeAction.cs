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

    public class ChangeDeviceVolumeAction : BaseAction
    {
        public Device Device { get; set; }
        public ChangeDeviceVolumeActionKind Operation { get; set; }
        public double Volume { get; set; }

        public ChangeDeviceVolumeAction()
        {
            DisplayName = "Change a device volume or mute";
            Options = new List<Option>
            {
                new Option("mute", ChangeDeviceVolumeActionKind.Mute),
                new Option("set volume", ChangeDeviceVolumeActionKind.SetVolume),
                new Option("toggle mute", ChangeDeviceVolumeActionKind.ToggleMute),
                new Option("unmute", ChangeDeviceVolumeActionKind.Unmute),
            };
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

        public override void Loaded()
        {
            var selected = Options.First(o => (ChangeDeviceVolumeActionKind)o.Value == Operation);
            Option = selected.Value;

            if (Operation == ChangeDeviceVolumeActionKind.SetVolume)
            {
                DisplayName = $"Set volume to {Math.Round(Volume)}% on {Device}";
            }
            else
            {
                DisplayName = $"Set {selected} on {Device}";
            }
        }
    }
}
