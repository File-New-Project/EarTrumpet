using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet.UI.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            //load languag
            foreach (ComboBoxItem item in ComboBoxLanguages.Items)
            {
                if (item.Tag.ToString().Equals(Services.SettingsService.Language))
                {
                    item.IsSelected = true;
                    chengeL = true;
                    break;
                }
            }
        }
        private readonly bool chengeL ;
        private void ComboBoxLanguages_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (chengeL)
            {
                ComboBoxItem mi = ComboBoxLanguages.SelectedItem as ComboBoxItem;// e.Source as ComboBoxItem;
                                                                                 //mi.IsChecked = true;
                App._Language.SwitchLanguage(mi.Tag.ToString());
                if (mi.Content.ToString() == "Auto")
                {
                    App._Language.SwitchLanguage(System.Globalization.CultureInfo.InstalledUICulture.Name);
                }
                Services.SettingsService.Language = mi.Tag.ToString();
            }

        }
    }
}
