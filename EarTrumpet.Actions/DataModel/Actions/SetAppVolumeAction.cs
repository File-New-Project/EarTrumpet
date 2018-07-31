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
                return $"Set volume to {Math.Round(Volume)}% for {App} on {Device}";
            }
            else
            {
                return $"Set {Options[0].DisplayName} for {App} on {Device}";
            }
        }
    }
}
