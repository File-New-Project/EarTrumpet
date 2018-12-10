using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetAppMuteAction : BaseAction, IPartWithDevice, IPartWithApp
    {
        public Device Device { get; set; }
        public App App { get; set; }
        public MuteKind Option { get; set; }
    }
}
