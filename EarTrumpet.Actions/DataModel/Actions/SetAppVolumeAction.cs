using EarTrumpet_Actions.DataModel.Enum;
using System;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetAppVolumeAction : BaseAction, IPartWithVolume, IPartWithDevice, IPartWithApp
    {
        public Device Device { get; set; }
        public App App { get; set; }
        public StreamActionKind Option { get; set; }
        public double Volume { get; set; }

        public SetAppVolumeAction()
        {
            Description = Properties.Resources.SetAppVolumeActionDescriptionText;
            Options = new List<OptionCollection>(new OptionCollection[]{ new OptionCollection(new List<Option>
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
                return string.Format(Properties.Resources.SetAppVolumeActionDescribeSetVolumeFormatText,
                    Math.Round(Volume), App, Device);
            }
            else
            {
                return string.Format(Properties.Resources.SetAppVolumeActionDescribeValueFormatText,
                    Options[0].DisplayName, App, Device);
            }
        }
    }
}
