using System.Windows;
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

        protected override void OnGotFocus(RoutedEventArgs e)
        {
           // base.OnGotFocus(e);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            //base.OnGotKeyboardFocus(e);
        }
    }
}
