using System;
using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;

namespace EarTrumpet.Extensibility
{
    public interface IEarTrumpetAddonDeviceContent
    {
        object GetContentForDevice(string deviceId, Action requestClose);
        IEnumerable<ContextMenuItem> GetContextMenuItemsForDevice(string deviceId);
    }
}
