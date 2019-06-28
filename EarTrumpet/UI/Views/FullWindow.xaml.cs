using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
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

            InitializeComponent();
            SourceInitialized += OnSourceInitialized;

            _windowAndItemSize = (double)App.Current.Resources["WindowAndItemSize"];

            // Auto-size on the first layout pass.
            SizeToContent = SizeToContent.WidthAndHeight;
            MaxWidth = FullWindowViewModel.SmallDeviceCountLimit * _windowAndItemSize;
            ContentRendered += (_, __) =>
            {
                // Update the size as devices change:
                ViewModel.AllDevices.CollectionChanged += OnDevicesChanged;
                OnDevicesChanged(null, null);
            };
            Themes.Manager.Current.ThemeChanged += SetBlurColor;
        }

        private void OnDevicesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ViewModel.AllDevices.Count == FullWindowViewModel.SmallDeviceCountLimit + 1 && e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
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

        private void SetBlurColor()
        {
            AccentPolicyLibrary.EnableAcrylic(this, Themes.Manager.Current.ResolveRef(this, "AcrylicColor_Settings"), Interop.User32.AccentFlags.DrawAllBorders);
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            this.Cloak();
            SetBlurColor();
        }
    }
}
