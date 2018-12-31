using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet.UI.Views
{
    public partial class FullWindow : Window
    {
        private FullWindowViewModel ViewModel => (FullWindowViewModel)DataContext;
        private bool _isClosing;

        public FullWindow()
        {
            Trace.WriteLine("FullWindow .ctor");

            InitializeComponent();

            SourceInitialized += FullWindow_SourceInitialized;
            LocationChanged += FullWindow_LocationChanged;
            SizeChanged += FullWindow_SizeChanged;
            PreviewKeyDown += FullWindow_PreviewKeyDown;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            Trace.WriteLine("FullWindow SystemEvents_DisplaySettingsChanged");

            Dispatcher.BeginInvoke((Action)(() => ViewModel.Dialog.IsVisible = false));
        }

        private void FullWindow_SourceInitialized(object sender, EventArgs e)
        {
            Trace.WriteLine("FullWindow FullWindow_SourceInitialized");

            this.Cloak();
            AccentPolicyLibrary.SetWindowBlur(this, true, true);
        }

        private void FullWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel.Dialog.IsVisible = false;
        }

        private void FullWindow_LocationChanged(object sender, EventArgs e)
        {
            ViewModel.Dialog.IsVisible = false;
        }

        private void FullWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (ViewModel.Dialog.IsVisible)
                {
                    ViewModel.Dialog.IsVisible = false;
                }
                else
                {
                    CloseButton_Click(null, null);
                }
            }
            else
            {
                KeyboardNavigator.OnKeyDown(this, ref e);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("FullWindow CloseButton_Click");
            if (!_isClosing)
            {
                // Ensure we don't double-animate if the user is able to close us multiple ways before the window stops accepting input.
                _isClosing = true;
                WindowAnimationLibrary.BeginWindowExitAnimation(this, () => this.Close());
            }
        }

        private void LightDismissBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.Dialog.IsVisible = false;
            e.Handled = true;
        }
    }
}
