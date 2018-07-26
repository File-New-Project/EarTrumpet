using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using System.Diagnostics;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class SettingsWindow : Window
    {
        private bool _isClosing;

        public SettingsWindow()
        {
            Trace.WriteLine("SettingsWindow .ctor");

            InitializeComponent();

            SourceInitialized += SettingsWindow_SourceInitialized;

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }
        
        private void SettingsWindow_SourceInitialized(object sender, System.EventArgs e)
        {
            Trace.WriteLine("SettingsWindow SettingsWindow_SourceInitialized");

            this.Cloak();
            AccentPolicyLibrary.SetWindowBlur(this, true, true);
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("SettingsWindow CloseButton_Click");

            if (!_isClosing)
            {
                // Ensure we don't double-animate if the user is able to close us multiple ways before the window stops accepting input.
                _isClosing = true;
                WindowAnimationLibrary.BeginWindowExitAnimation(this, () => this.Close());
            }
        }
    }
}
