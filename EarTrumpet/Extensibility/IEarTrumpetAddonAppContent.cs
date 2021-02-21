using System;
using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;

namespace EarTrumpet.Extensibility
{
    public interface IEarTrumpetAddonAppContent
    {
        object GetContentForApp(string deviceId, string appId, Action requestClose);
        IEnumerable<ContextMenuItem> GetContextMenuItemsForApp(string deviceId, string appId);
    }
}
