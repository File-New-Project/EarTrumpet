using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetDeviceVolumeAction : BaseAction, IPartWithDevice, IPartWithVolume
    {
        public Device Device { get; set; }
        public SetVolumeKind Option { get; set; }
        public double Volume { get; set; }

        public SetDeviceVolumeAction()
        {
            Description = Properties.Resources.SetDeviceVolumeActionDescriptionText;
            Options = new List<OptionCollection>(new OptionCollection[]{ new OptionCollection(new List<Option>
                {
                    new Option(Properties.Resources.StreamActionKindSetVolumeText, SetVolumeKind.Set),
                    new Option(Properties.Resources.StreamActionKindIncrement5Text, SetVolumeKind.Increment),
                    new Option(Properties.Resources.StreamActionKindDecrement5Text, SetVolumeKind.Decrement),
                },
                (newValue) => Option = (SetVolumeKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => Properties.Resources.SetDeviceVolumeActionDescribeSetVolumeFormatText;
    }
}
