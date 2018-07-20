using System;
using System.Collections.Generic;

namespace EarTrumpet.Extensibility
{
    public interface IContextMenuItems
    {
        string DisplayName { get; }
        List<Tuple<string, Action>> Items { get; }
    }
}
