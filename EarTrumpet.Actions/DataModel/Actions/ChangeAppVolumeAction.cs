using EarTrumpet.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class ChangeAppVolumeAction : BaseAction, IPartWithVolume, IPartWithDevice, IPartWithApp
    {
        public Device Device { get; set; }
        public App DeviceSession { get; set; }
        public ChangeDeviceVolumeActionKind Operation { get; set; }
        public double Volume { get; set; }

        public ChangeAppVolumeAction()
        {
            Description = "Change an app volume or mute";
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
                return $"Set volume to {Math.Round(Volume)}% for {DeviceSession} on {Device}";
            }
            else
            {
                return $"Set {Options[0].DisplayName} for {DeviceSession} on {Device}";
            }
        }
    }
}
