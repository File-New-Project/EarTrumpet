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

    public class SetDeviceVolumeAction : BaseAction, IPartWithDevice, IPartWithVolume
    {
        public Device Device { get; set; }
        public StreamActionKind Option { get; set; }
        public double Volume { get; set; }

        public SetDeviceVolumeAction()
        {
            Description = Properties.Resources.SetDeviceVolumeActionDescriptionText;
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                    new Option(Properties.Resources.StreamActionKindMuteText, StreamActionKind.Mute),
                    new Option(Properties.Resources.StreamActionKindSetVolumeText, StreamActionKind.SetVolume),
                    new Option(Properties.Resources.StreamActionKindToggleMuteText, StreamActionKind.ToggleMute),
                    new Option(Properties.Resources.StreamActionKindUnuteText, StreamActionKind.Unmute),
                    new Option(Properties.Resources.StreamActionKindIncrement5Text, StreamActionKind.Increment5),
                    new Option(Properties.Resources.StreamActionKindDecrement5Text, StreamActionKind.Decrement5),
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
