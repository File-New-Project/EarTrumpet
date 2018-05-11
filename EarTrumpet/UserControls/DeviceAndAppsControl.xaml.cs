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
        Style _focusVisualStyle;

        const string s_DragDropDataFormat = "EarTrumpet.AudioSession";

        public DeviceViewModel Device { get { return (DeviceViewModel)GetValue(DeviceProperty); } set { SetValue(DeviceProperty, value); } }
        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(DeviceViewModel), typeof(DeviceAndAppsControl), new PropertyMetadata(null));

        public DeviceAndAppsControl()
        {
            InitializeComponent();
            GridRoot.DataContext = this;

            UpdateTheme();
            ThemeService.ThemeChanged += UpdateTheme;
        }

        ~DeviceAndAppsControl()
        {
            ThemeService.ThemeChanged -= UpdateTheme;
        }

        void UpdateTheme()
        {
            ThemeService.UpdateThemeResources(Resources);
        }

        private void ListViewItem_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var lvi = ((ListViewItem)sender);
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
                    DeviceListItem.FocusVisualStyle = _focusVisualStyle;

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
                    ShowFocus();

                    if (AppList.ItemContainerGenerator.IndexFromContainer(lvi) == 0)
                    {
                        // When we're the first ListViewItem in the list, move focus like shift+tab to the previous item.
                        DeviceListItem.Focus();
                        e.Handled = true;
                    }
                }
                else if (e.Key == Key.Down)
                {
                    ShowFocus();

                    if (AppList.ItemContainerGenerator.IndexFromContainer(lvi) == AppList.Items.Count - 1)
                    {
                        // When we're the first ListViewItem in the list, move focus like shift+tab to the previous item.
                        lvi.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        e.Handled = true;
                    }
                }
            }
        }

        internal void ShowFocus()
        {
            DeviceListItem.FocusVisualStyle = _focusVisualStyle;
        }

        internal void HideFocus()
        {
            if (_focusVisualStyle == null && DeviceListItem.FocusVisualStyle != null)
            {
                _focusVisualStyle = DeviceListItem.FocusVisualStyle;
            }

            DeviceListItem.FocusVisualStyle = null;
            DeviceListItem.Focus();
        }

        private void MoveToAnotherDevice_Click(object sender, RoutedEventArgs e)
        {
            var selectedApp = MainViewModel.ExpandedApp;
            var vm = MainViewModel.Instance;
            
            var currentDeviceId = selectedApp.Session.Device.Id;

            var moveMenu = new ContextMenu();

            var addDevice = new Action<DeviceViewModel>((device) =>
            {
                if (device.Device.Id == currentDeviceId) return;

                var newItem = new MenuItem { Header = device.Device.DisplayName };
                newItem.Click += (_, __) =>
                {
                    device.TakeExternalSession(selectedApp);
                    selectedApp.IsExpanded = false;
                    MainViewModel.ExpandedApp = null;
                };
                moveMenu.Items.Add(newItem);
            });

            foreach (var device in vm.Devices)
            {
                addDevice(device);
            }

            addDevice(vm.DefaultDevice);

            moveMenu.PlacementTarget = (UIElement)sender;
            moveMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            moveMenu.IsOpen = true;
        }
    }
}
