using EarTrumpet.Interop.Helpers;
using EarTrumpet.Properties;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EarTrumpet
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Window
    {
        public Welcome()
        {
            InitializeComponent();
            this.Loaded += Welcome_Loaded;
        }

        private void Welcome_Loaded(object sender, RoutedEventArgs e)
        {
            AccentPolicyLibrary.SetWindowBlur(this, true, false);
            Show();
            Hide();
            Show();
            SettingsService.Welcome = false;
            //M1.LoadSmile(Properties.Resource1.welcome);
            //this.Background.Changed += Background_Changed;
            //G1.Background = this.Background;
            //this.Background = null;
            //Bolback.Background.Changed += Background_Changed;
            //Windows.UI.Xaml.Controls.Image image = new Windows.UI.Xaml.Controls.Image();
            //image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("/مدیریت صدا;component/Assets/Welcome.gif")); ;
            //Image image2 = new Image();

        }

        private void Background_Changed(object sender, EventArgs e)
        {
            Bolback.Background.Changed -= Background_Changed;
            G1.Background = this.Background;
            this.Background = null;

            //SolidColorBrush c = Bolback.Background as SolidColorBrush;
            //var a1 = c.Color;
            //a1.A = 80;
            //SolidColorBrush cc = new SolidColorBrush(a1);
            //Bolback.Background = cc;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            //Application.Current.Shutdown();
        }
    }
}
