using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel
{
    interface IPartWithOption
    {
        object Option { get; set; }
        List<Option> Options { get; }
    }
}
