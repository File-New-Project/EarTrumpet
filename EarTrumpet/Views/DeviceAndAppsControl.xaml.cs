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
            DependencyProperty.Register("Device", typeof(DeviceViewModel), typeof(DeviceAndAppsControl), new PropertyMetadata(null));

        public DeviceAndAppsControl()
        {
            InitializeComponent();
            GridRoot.DataContext = this;
        }

        private void Mute_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Device.Device.IsMuted = !Device.Device.IsMuted;
                e.Handled = true;
            }
        }

        private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Device.Device.IsMuted = !Device.Device.IsMuted;
                e.Handled = true;
            }
        }

        private void TouchSlider_TouchUp(object sender, TouchEventArgs e)
        {
            System.Media.SystemSounds.Beep.Play();
        }

        private void TouchSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Media.SystemSounds.Beep.Play();
        }

        private void AppVolumeControl_AppExpanded(object sender, AppVolumeControlExpandedEventArgs e)
        {
            AppExpanded?.Invoke(sender, e);
        }
    }
}
