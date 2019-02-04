using System.Windows.Input;

namespace EarTrumpet.UI.Controls
{
    public class ListViewItem : System.Windows.Controls.ListViewItem
    {
        ListView _parent;
        public ListViewItem(ListView parent)
        {
            _parent = parent;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            _parent.InvokeItem(this);
            base.OnMouseDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _parent.InvokeItem(this);
            }
            base.OnKeyDown(e);
        }
    }
}
