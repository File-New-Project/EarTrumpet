using EarTrumpet_Actions.DataModel.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetDeviceMuteAction : BaseAction, IPartWithDevice
    {
        public Device Device { get; set; }
        public MuteKind Option { get; set; }

        public SetDeviceMuteAction()
        {
            Description = Properties.Resources.SetDeviceMuteActionDescriptionText;
            Options = new List<OptionCollection>(new OptionCollection[]{ new OptionCollection(new List<Option>
                {
                    new Option(Properties.Resources.StreamActionKindMuteText, MuteKind.Mute),
                    new Option(Properties.Resources.StreamActionKindToggleMuteText, MuteKind.ToggleMute),
                    new Option(Properties.Resources.StreamActionKindUnuteText, MuteKind.Unmute),
                },
                (newValue) => Option = (MuteKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => Properties.Resources.SetDeviceVolumeActionDescribeValueFormatText;
    }
}
