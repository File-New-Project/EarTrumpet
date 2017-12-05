using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet.UserControls
{
    public partial class DeviceAndAppsControl : UserControl
    {
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

        void UpdateTheme()
        {
            ThemeService.UpdateThemeResources(Resources);
        }
    }
}
