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
                }
            }
            else
            {
                // These apply to all the app sessions.
                var vm = (AudioSessionViewModel)dt;

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
                        lvi.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                        e.Handled = true;
                    }

                    ShowFocus();
                }
                else if (e.Key == Key.Down)
                {
                    if (AppList.ItemContainerGenerator.IndexFromContainer(lvi) == AppList.Items.Count - 1)
                    {
                        // When we're the first ListViewItem in the list, move focus like shift+tab to the previous item.
                        lvi.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        e.Handled = true;
                    }

                    ShowFocus();
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
    }
}
