using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.UI.ViewModels;
using System.Diagnostics;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class FullWindow : Window
    {
        private readonly double _windowAndItemSize;
        private FullWindowViewModel ViewModel => (FullWindowViewModel)DataContext;

        public FullWindow()
        {
            Trace.WriteLine("FullWindow .ctor");
            Closed += (_, __) => Trace.WriteLine("FullWindow Closed");

            _windowAndItemSize = (double)App.Current.Resources["WindowAndItemSize"];

            InitializeComponent();
            SourceInitialized += (_, __) =>
            {
                this.Cloak();
                this.EnableRoundedCornersIfApplicable();
            };

            // Auto-size on the first layout pass.
            SizeToContent = SizeToContent.WidthAndHeight;
            MaxWidth = FullWindowViewModel.SmallDeviceCountLimit * _windowAndItemSize;
            ContentRendered += (_, __) =>
            {
                // Update the size as devices change:
                ViewModel.AllDevices.CollectionChanged += OnDevicesChanged;
                OnDevicesChanged(null, null);
            };
        }

        private void OnDevicesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ViewModel.AllDevices.Count == FullWindowViewModel.SmallDeviceCountLimit + 1 && e != null && e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                // We're growing from auto-size to fixed-size, we need to make space for the padding at the bottom.
                MaxWidth = FullWindowViewModel.SmallDeviceCountLimit * _windowAndItemSize;
                UpdateLayout();
            }

            if (ViewModel.AllDevices.Count == 0)
            {
                // No devices panel with auto layout.
                MaxWidth = double.PositiveInfinity;
                MinHeight = 0;
            }
            else
            {
                // Pick a value so the user can't make the window too small by accident.
                MinHeight = 200;
                // Don't let the user expand past the last device.
                MaxWidth = 2 /* Window borders */ + 
                    ViewModel.AllDevices.Count * _windowAndItemSize;
            }

            SizeToContent = ViewModel.IsManyDevicesMode ? SizeToContent.Manual : SizeToContent.WidthAndHeight;
            ResizeMode = ViewModel.IsManyDevicesMode ? ResizeMode.CanResize : ResizeMode.NoResize;
            this.RemoveWindowStyle(User32.WS_MAXIMIZEBOX);
        }
    }
}
