using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;

namespace EarTrumpet.Extensibility
{
    public interface IAddonAppContextMenu
    {
        IEnumerable<ContextMenuItem> GetItemsForApp(string deviceId, string appId);
    }
}
