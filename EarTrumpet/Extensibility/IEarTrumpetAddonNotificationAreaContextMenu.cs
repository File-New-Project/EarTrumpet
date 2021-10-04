using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;

namespace EarTrumpet.Extensibility
{
    public interface IEarTrumpetAddonNotificationAreaContextMenu
    {
        IEnumerable<ContextMenuItem> NotificationAreaContextMenuItems { get; }
    }
}