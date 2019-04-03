using System;
using System.Windows;

namespace EarTrumpet.UI.Controls
{
    public class ListView : System.Windows.Controls.ListView
    {
        public event EventHandler<object> ItemInvoked;

        protected override DependencyObject GetContainerForItemOverride() => new ListViewItem(this);

        public void InvokeItem(ListViewItem listViewItem)
        {
            var viewModel = ItemContainerGenerator.ItemFromContainer(listViewItem);
            ItemInvoked?.Invoke(this, viewModel);
        }
    }
}
