using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetAppMuteAction : BaseAction, IPartWithDevice, IPartWithApp
    {
        public Device Device { get; set; }
        public App App { get; set; }
        public MuteKind Option { get; set; }

        public SetAppMuteAction()
        {
            Description = Properties.Resources.SetAppMuteActionDescriptionText;
            Options = new List<OptionCollection>(new OptionCollection[]{ new OptionCollection(new List<Option>
                {
                    new Option(Properties.Resources.StreamActionKindMuteText, MuteKind.Mute),
                    new Option(Properties.Resources.StreamActionKindToggleMuteText, MuteKind.ToggleMute),
                    new Option(Properties.Resources.StreamActionKindUnuteText, MuteKind.Unmute),
                },
                (newValue) => Option = (MuteKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => Properties.Resources.SetAppVolumeActionDescribeValueFormatText;
    }
}
