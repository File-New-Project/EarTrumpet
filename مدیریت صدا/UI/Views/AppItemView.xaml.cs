using EarTrumpet.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet.UI.Views
{
    public partial class AppItemView : UserControl
    {
        private AppItemViewModel App => (AppItemViewModel)DataContext;

        public AppItemView()
        {
            InitializeComponent();

            PreviewMouseRightButtonUp += AppVolumeControl_PreviewMouseRightButtonUp;
        }

        private void AppVolumeControl_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ExpandApp();
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            App.IsMuted = !App.IsMuted;
            e.Handled = true;
        }

        public void ExpandApp()
        {
            App.OpenPopup(this);
        }

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            App.RefreshDisplayName();
        }
    }
}
