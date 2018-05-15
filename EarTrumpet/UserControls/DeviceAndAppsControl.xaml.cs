using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EarTrumpet.UserControls
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

        private void ListViewItem_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var lvi = ((AppVolumeControl)sender);
            var dt = lvi.DataContext;

            if (dt is DeviceAndAppsControl)
            {
                // These apply only to the device ListViewItem

                if (e.Key == Key.M || e.Key == Key.Space)
                {
                    Device.Device.IsMuted = !Device.Device.IsMuted;
                    e.Handled = true;
                }
                else if (e.Key == Key.Left)
                {
                    Device.Device.Volume--;
                    e.Handled = true;
                }
                else if (e.Key == Key.Right)
                {
                    Device.Device.Volume++;
                    e.Handled = true;
                }
                else if (e.Key == Key.Tab || e.Key == Key.Down || e.Key == Key.Up)
                {
                    if (e.Key == Key.Down)
                    {
                        if (AppList.Items.Count > 0)
                        {
                            ((FrameworkElement)AppList.ItemContainerGenerator.ContainerFromItem(AppList.Items[0])).Focus();
                            e.Handled = true;
                        }
                    }
                }
            }
            else
            {
                // These apply to all the app sessions.
                var vm = (AppItemViewModel)dt;

                if (e.Key == Key.M || e.Key == Key.Space)
                {
                    vm.IsMuted = !vm.IsMuted;
                    e.Handled = true;
                }
                else if (e.Key == Key.Left)
                {
                    vm.Volume--;
                    e.Handled = true;
                }
                else if (e.Key == Key.Right)
                {
                    vm.Volume++;
                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    if (AppList.ItemContainerGenerator.IndexFromContainer(lvi) == 0)
                    {
                        // When we're the first ListViewItem in the list, move focus like shift+tab to the previous item.
                        DeviceListItem.Focus();
                        e.Handled = true;
                    }
                }
                else if (e.Key == Key.Down)
                {
                    if (AppList.ItemContainerGenerator.IndexFromContainer(lvi) == AppList.Items.Count - 1)
                    {
                        // When we're the first ListViewItem in the list, move focus like shift+tab to the previous item.
                        lvi.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        e.Handled = true;
                    }
                }
            }
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
