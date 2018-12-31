using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;

namespace EarTrumpet.Extensibility
{
    public interface IAddonContextMenu
    {
        IEnumerable<ContextMenuItem> Items { get; }
    }
}