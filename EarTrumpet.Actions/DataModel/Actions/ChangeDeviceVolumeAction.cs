using System;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public enum StreamActionKind
    {
        Mute,
        Unmute,
        SetVolume,
        ToggleMute,
        Increment5,
        Decrement5,
    }

    public class ChangeDeviceVolumeAction : BaseAction, IPartWithDevice, IPartWithVolume
    {
        public Device Device { get; set; }
        public StreamActionKind Option { get; set; }
        public double Volume { get; set; }

        public ChangeDeviceVolumeAction()
        {
            Description = "Set a device volume or mute";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                new Option("mute", StreamActionKind.Mute),
                new Option("set volume", StreamActionKind.SetVolume),
                new Option("toggle mute", StreamActionKind.ToggleMute),
                new Option("unmute", StreamActionKind.Unmute),
                new Option("increment volume by 5%", StreamActionKind.Increment5),
                new Option("decrement volume by 5%", StreamActionKind.Decrement5),
                },
                (newValue) => Option = (StreamActionKind)newValue.Value,
                () => Option) });
        }

        public override string Describe()
        {
            if (Option == StreamActionKind.SetVolume)
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
