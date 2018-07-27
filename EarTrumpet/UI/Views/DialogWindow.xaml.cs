using EarTrumpet.DataModel;
using EarTrumpet.Interop.Helpers;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();

            SourceInitialized += (_, __) => AccentPolicyLibrary.SetWindowBlur(this, true, true);

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
