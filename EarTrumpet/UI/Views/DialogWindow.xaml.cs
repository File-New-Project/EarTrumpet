using EarTrumpet.DataModel;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using System.Diagnostics;
using System.Windows;
namespace EarTrumpet.UI.Views
{
    public partial class DialogWindow : Window
    {
        private bool _isClosing;

        public DialogWindow()
        {
            Trace.WriteLine("DialogWindow .ctor");

            InitializeComponent();

            SourceInitialized += OnSourceInitialized;
        }

        private void OnSourceInitialized(object sender, System.EventArgs e)
        {
            Trace.WriteLine("DialogWindow OnSourceInitialized");
            AccentPolicyLibrary.EnableAcrylic(this, Themes.Manager.Current.ResolveRef(this, "AcrylicColor_Settings"), Interop.User32.AccentFlags.DrawAllBorders);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("DialogWindow CloseButton_Click");
            e.Handled = true;

            SafeClose();
        }

        public void SafeClose()
        {
            Trace.WriteLine("DialogWindow SafeClose");

            if (!_isClosing)
            {
                // Ensure we don't double-animate if the user is able to close us multiple ways before the window stops accepting input.
                _isClosing = true;
                WindowAnimationLibrary.BeginWindowExitAnimation(this, () => this.Close());
            }
        }
    }
}
