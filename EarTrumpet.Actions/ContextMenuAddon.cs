using EarTrumpet.Extensibility;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
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
                if (Addon.Current.Actions.Any())
                {
                    ret.Add(new ContextMenuItem
                    {
                        DisplayName = Properties.Resources.MyActionsText,
                        Children = Addon.Current.Actions.
                        Select(a => new ContextMenuItem
                        {
                            DisplayName = a.DisplayName,
                            Command = new RelayCommand(() => Addon.Current.TriggerAction(a))
                        })
                    });
                }
                return ret;
            }
        }
    }
}
