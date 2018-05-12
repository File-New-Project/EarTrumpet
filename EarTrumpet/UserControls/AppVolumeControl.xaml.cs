using EarTrumpet.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EarTrumpet.UserControls
{
    public partial class AppVolumeControl : UserControl
    {
        public AppItemViewModel App { get { return (AppItemViewModel)GetValue(StreamProperty); } set { SetValue(StreamProperty, value); } }
        public static readonly DependencyProperty StreamProperty = DependencyProperty.Register(
          "App", typeof(AppItemViewModel), typeof(AppVolumeControl), new PropertyMetadata(null));

        public AppVolumeControl()
        {
            InitializeComponent();
            GridRoot.DataContext = this;
        }

        private void Mute_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                App.IsMuted = !App.IsMuted;
                e.Handled = true;
            }
        }

        private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                App.IsMuted = !App.IsMuted;
                e.Handled = true;
            }
        }

        private void UserControl_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!App.IsExpanded)
            {
                App.IsExpanded = true;
                var oldApp = MainViewModel.ExpandedApp;
                MainViewModel.ExpandedApp = App;

                if (oldApp != null && oldApp != App)
                {
                    oldApp.IsExpanded = false;
                }
            }
        }
    }
}
