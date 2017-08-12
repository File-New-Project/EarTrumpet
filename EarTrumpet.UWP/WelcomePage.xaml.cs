using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EarTrumpet.UWP
{
    public sealed partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            this.InitializeComponent();
        }

        private void Close_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
