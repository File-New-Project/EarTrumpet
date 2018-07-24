using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace EarTrumpet.UI.Views
{
    public partial class ModalDialogChrome : UserControl
    {
        public ModalDialogChrome()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var btn = (Button)sender;

            btn.ContextMenu.Opened += (_, __) =>
            {
                ((Popup)btn.ContextMenu.Parent).PopupAnimation = PopupAnimation.None;
            };

            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.Placement = PlacementMode.Bottom;
            btn.ContextMenu.IsOpen = true;
        }
    }
}
