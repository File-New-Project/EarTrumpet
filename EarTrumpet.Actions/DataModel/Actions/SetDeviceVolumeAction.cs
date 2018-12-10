using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetDeviceVolumeAction : BaseAction, IPartWithDevice, IPartWithVolume
    {
        public Device Device { get; set; }
        public SetVolumeKind Option { get; set; }
        public double Volume { get; set; }
    }
}
