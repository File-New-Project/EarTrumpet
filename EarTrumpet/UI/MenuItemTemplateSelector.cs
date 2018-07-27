using EarTrumpet.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet.UI
{
    public enum ThemeKind
    {
        LightOrDark,
        DarkOnly
    }

    public class MenuItemTemplateSelector : ItemContainerTemplateSelector
    {
        public ThemeKind Theme { get; set; }

        public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
        {
            var key = "";
            if (item is ContextMenuSeparator)
            {
                key = "ContextMenuSeparatorTemplate";
            }
            else if (item is ContextMenuItem && ((ContextMenuItem)item).Children != null)
            {
                key = "ContextMenuSubItemTemplate";
            }
            else if (item is ContextMenuItem )
            {
                key = "ContextMenuItemTemplate";
            }
            else throw new NotImplementedException();

            if (Theme == ThemeKind.DarkOnly)
            {
                key += "DarkOnly";
            }

            return (DataTemplate)parentItemsControl.FindResource(key);
        }
    }
}
