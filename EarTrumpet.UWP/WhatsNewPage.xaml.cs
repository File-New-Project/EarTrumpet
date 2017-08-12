using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EarTrumpet.UWP
{
    public sealed partial class WhatsNewPage : Page
    {
        private Dictionary<string, string> _whatsNewItems;

        public WhatsNewPage()
        {
            this.InitializeComponent();
            
            LoadWhatsNew();
        }

        private void LoadWhatsNew()
        {
            _whatsNewItems = new Dictionary<string, string>();
            var resources = ResourceLoader.GetForCurrentView();

            var title = resources.GetString("WhatsNew/Text");
            WhatsNew.Text = string.Format(title, GetAppVersion());

            var item1 = resources.GetString("NewItem1/Text");
            _whatsNewItems.Add(item1, null);

            var item2 = resources.GetString("NewItem2/Text");
            _whatsNewItems.Add(item2, null);

            var item3 = resources.GetString("NewItem3/Text");
            _whatsNewItems.Add(item3, null);

            var item4 = resources.GetString("NewItem4/Text");
            _whatsNewItems.Add(item4, null);

            var item5 = resources.GetString("NewItem5/Text");
            _whatsNewItems.Add(item5, null);
        }

        private string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        private void Close_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
