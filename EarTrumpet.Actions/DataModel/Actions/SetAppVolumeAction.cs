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
        }

        public override string Describe()
        {
            if (Option == SetVolumeKind.Set)
            {
                return Properties.Resources.SetAppVolumeActionDescribeSetVolumeFormatText;
            }
            else
            {
                return Properties.Resources.SetAppVolumeActionDescribeSetVolumeIncrementFormatText;
            }
        }
    }
}
