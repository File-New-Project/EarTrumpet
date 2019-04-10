using System;
using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;

namespace EarTrumpet.Extensibility
{
    public interface IAddonAppContent
    {
        object GetContentForApp(string deviceId, string appId, Action requestClose);
        IEnumerable<ContextMenuItem> GetItemsForApp(string deviceId, string appId);
    }
}
