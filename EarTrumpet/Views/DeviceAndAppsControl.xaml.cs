using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EarTrumpet.Views
{
    public partial class DeviceAndAppsControl : UserControl
    {
        public event EventHandler<AppVolumeControlExpandedEventArgs> AppExpanded;

        public DeviceViewModel Device { get { return (DeviceViewModel)GetValue(DeviceProperty); } set { SetValue(DeviceProperty, value); } }
        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(DeviceViewModel), typeof(DeviceAndAppsControl), new PropertyMetadata(new PropertyChangedCallback(DeviceChanged)));

        public DeviceAndAppsControl()
        {
            InitializeComponent();
        }

        private static void DeviceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (DeviceAndAppsControl)d;
            self.GridRoot.DataContext = self.Device;
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

        private void AppVolumeControl_AppExpanded(object sender, AppVolumeControlExpandedEventArgs e)
        {
            AppExpanded?.Invoke(sender, e);
        }
    }
}
