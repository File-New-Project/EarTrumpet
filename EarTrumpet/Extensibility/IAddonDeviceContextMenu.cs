using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;

namespace EarTrumpet.Extensibility
{
    public interface IAddonDeviceContextMenu
    {
        IEnumerable<ContextMenuItem> GetItemsForDevice(string deviceId);
    }
}
