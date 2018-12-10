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
    }
}
