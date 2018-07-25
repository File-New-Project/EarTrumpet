using System;
using System.Collections.Generic;

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
