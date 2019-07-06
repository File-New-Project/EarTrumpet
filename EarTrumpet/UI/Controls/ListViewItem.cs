using System.Windows.Input;

namespace EarTrumpet.UI.Controls
{
    public class ListViewItem : System.Windows.Controls.ListViewItem
    {
        private readonly ListView _parent;

        public ListViewItem(ListView parent)
        {
            _parent = parent;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            _parent.InvokeItem(this);
            base.OnMouseUp(e);
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
