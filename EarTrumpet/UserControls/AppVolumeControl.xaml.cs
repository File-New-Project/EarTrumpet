using EarTrumpet.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet.UserControls
{
    public partial class AppVolumeControl : UserControl
    {
        public event EventHandler<AppVolumeControlExpandedEventArgs> AppExpanded;

        public AppItemViewModel App { get { return (AppItemViewModel)GetValue(StreamProperty); } set { SetValue(StreamProperty, value); } }
        public static readonly DependencyProperty StreamProperty = DependencyProperty.Register(
          "App", typeof(AppItemViewModel), typeof(AppVolumeControl), new PropertyMetadata(null));

        public AppVolumeControl()
        {
            InitializeComponent();
            GridRoot.DataContext = this;
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            App.IsMuted = !App.IsMuted;
            e.Handled = true;
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (!App.IsExpanded)
            {
                AppExpanded?.Invoke(this, new AppVolumeControlExpandedEventArgs
                {
                    ViewModel = App,
                    Container = (UIElement)sender,
                });
            }
        }
    }

    public class AppVolumeControlExpandedEventArgs
    {
        public AppItemViewModel ViewModel;
        public UIElement Container;
    }
}
