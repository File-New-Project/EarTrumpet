using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetAppVolumeAction : BaseAction, IPartWithVolume, IPartWithDevice, IPartWithApp
    {
        public Device Device { get; set; }
        public App App { get; set; }
        public SetVolumeKind Option { get; set; }
        public double Volume { get; set; }

        public SetAppVolumeAction()
        {
            Description = Properties.Resources.SetAppVolumeActionDescriptionText;
            Options = new List<OptionCollection>(new OptionCollection[]{ new OptionCollection(new List<Option>
                {
                    new Option(Properties.Resources.StreamActionKindSetVolumeText, SetVolumeKind.Set),
                    new Option(Properties.Resources.StreamActionKindIncrement5Text, SetVolumeKind.Increment),
                    new Option(Properties.Resources.StreamActionKindDecrement5Text, SetVolumeKind.Decrement),
                },
                (newValue) => Option = (SetVolumeKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => Properties.Resources.SetAppVolumeActionDescribeSetVolumeFormatText;
    }
}
