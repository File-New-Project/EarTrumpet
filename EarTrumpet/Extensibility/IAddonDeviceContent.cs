using System;
using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;

namespace EarTrumpet.Extensibility
{
    public interface IAddonDeviceContent
    {
        object GetContentForDevice(string deviceId, Action requestClose);
        IEnumerable<ContextMenuItem> GetItemsForDevice(string deviceId);
    }
}
