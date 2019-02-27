using EarTrumpet.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet.UI.Views
{
    public partial class AppItemView : UserControl
    {
        private IAppItemViewModel App => (IAppItemViewModel)DataContext;

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
    }
}
