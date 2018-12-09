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
        }

        public override string Describe()
        {
            if (Option == SetVolumeKind.Set)
            {
                return Properties.Resources.SetDeviceVolumeActionDescribeSetVolumeFormatText;
            }
            else
            {
                return Properties.Resources.SetDeviceVolumeActionDescribeSetVolumeIncrementFormatText;
            }
        }
    }
}
