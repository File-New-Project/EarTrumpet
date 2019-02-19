using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EarTrumpet.UI.Views
{
    public partial class DeviceView : UserControl
    {
        public DeviceViewModel Device { get { return (DeviceViewModel)GetValue(DeviceProperty); } set { SetValue(DeviceProperty, value); } }
        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(DeviceViewModel), typeof(DeviceView), new PropertyMetadata(new PropertyChangedCallback(DeviceChanged)));

        public bool IsDisplayNameVisible { get { return (bool)GetValue(IsDisplayNameVisibleProperty); } set { SetValue(IsDisplayNameVisibleProperty, value); } }
        public static readonly DependencyProperty IsDisplayNameVisibleProperty =
            DependencyProperty.Register("IsDisplayNameVisible", typeof(bool), typeof(DeviceView), new PropertyMetadata(true));

        public DeviceView()
        {
            InitializeComponent();
        }

        private static void DeviceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (DeviceView)d;
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
            SystemSoundsHelper.PlayBeepSound.Execute(null);
        }

        private void TouchSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                SystemSoundsHelper.PlayBeepSound.Execute(null);
            }
        }

        private void DeviceListItem_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Device.OpenPopup(Device, DeviceListItem);
        }
    }
}
