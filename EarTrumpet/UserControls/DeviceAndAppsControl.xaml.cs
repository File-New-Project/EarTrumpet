using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EarTrumpet.UserControls
{
    public partial class DeviceAndAppsControl : UserControl
    {
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
            var dt = ((ListViewItem)sender).DataContext;

            if (dt is DeviceAndAppsControl)
            {
                if (e.Key == System.Windows.Input.Key.M)
                {
                    Device.Device.IsMuted = !Device.Device.IsMuted;
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.Left)
                {
                    Device.Device.Volume--;
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.Right)
                {
                    Device.Device.Volume++;
                    e.Handled = true;
                }
            }
            else
            {
                var vm = (AudioSessionViewModel)dt;

                if (e.Key == System.Windows.Input.Key.M)
                {
                    vm.IsMuted = !vm.IsMuted;
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.Left)
                {
                    vm.Volume--;
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.Right)
                {
                    vm.Volume++;
                    e.Handled = true;
                }
            }
        }
    }
}
