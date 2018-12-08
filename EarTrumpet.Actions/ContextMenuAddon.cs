using EarTrumpet.Extensibility;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel.Triggers;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace EarTrumpet_Actions
{
    [Export(typeof(IAddonContextMenu))]
    public class ContextMenuAddon : IAddonContextMenu
    {
        public IEnumerable<ContextMenuItem> Items
        {
            get
            {
                var ret = new List<ContextMenuItem>();
                ret.Add(new ContextMenuItem
                {
                    DisplayName = Properties.Resources.EditActionsAndHotkeysText,
                    Command = new RelayCommand(() => Addon.Current.OpenSettingsWindow())
                });

                var items = Addon.Current.Actions.Where(a => a.Triggers.FirstOrDefault(ax => ax is ContextMenuTrigger) != null);
                if (items.Any())
                {
                    ret.Add(new ContextMenuSeparator { });
                }

                foreach (var item in items)
                {
                    ret.Add(new ContextMenuItem
                    {
                        DisplayName = item.DisplayName,
                        Command = new RelayCommand(() => Addon.Current.TriggerAction(item))
                    });
                }
                return ret;
            }
        }
    }
}
