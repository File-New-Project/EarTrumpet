using EarTrumpet.Extensibility;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel.Serialization;
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
                foreach (var item in Addon.Current.Actions.Where(a => a.Triggers.FirstOrDefault(ax => ax is ContextMenuTrigger) != null))
                {
                    ret.Add(new ContextMenuItem
                    {
                        Glyph = "\xE00A",
                        IsChecked = true,
                        DisplayName = item.DisplayName,
                        Command = new RelayCommand(() => Addon.Current.TriggerAction(item))
                    });
                }
                return ret;
            }
        }
    }
}
