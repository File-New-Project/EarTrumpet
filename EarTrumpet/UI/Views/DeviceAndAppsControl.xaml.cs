using EarTrumpet.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EarTrumpet.UI.Views
{
    public partial class DeviceAndAppsControl : UserControl
    {
        public DeviceViewModel Device { get { return (DeviceViewModel)GetValue(DeviceProperty); } set { SetValue(DeviceProperty, value); } }
        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(DeviceViewModel), typeof(DeviceAndAppsControl), new PropertyMetadata(new PropertyChangedCallback(DeviceChanged)));

        public bool IsDisplayNameVisible { get { return (bool)GetValue(IsDisplayNameVisibleProperty); } set { SetValue(IsDisplayNameVisibleProperty, value); } }
        public static readonly DependencyProperty IsDisplayNameVisibleProperty =
            DependencyProperty.Register("IsDisplayNameVisible", typeof(bool), typeof(DeviceAndAppsControl), new PropertyMetadata(true));

        public DeviceAndAppsControl()
        {
            InitializeComponent();
        }

        private static void DeviceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (DeviceAndAppsControl)d;
            self.GridRoot.DataContext = self;
        }

        private void Mute_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Device.IsMuted = !Device.IsMuted;
                e.Handled = true;
            }
        }

        private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Device.IsMuted = !Device.IsMuted;
                e.Handled = true;
            }
        }

        private void TouchSlider_TouchUp(object sender, TouchEventArgs e)
        {
            System.Media.SystemSounds.Beep.Play();
        }

        private void TouchSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }
    }
}
