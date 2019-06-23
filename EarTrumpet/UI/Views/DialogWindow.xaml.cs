using EarTrumpet.Interop.Helpers;
using System.Diagnostics;
using System.Windows;
namespace EarTrumpet.UI.Views
{
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            Trace.WriteLine("DialogWindow .ctor");
            Closed += (_, __) => Trace.WriteLine("DialogWindow Closed");

            InitializeComponent();

            SourceInitialized += OnSourceInitialized;
        }

        private void OnSourceInitialized(object sender, System.EventArgs e)
        {
            AccentPolicyLibrary.EnableAcrylic(this, Themes.Manager.Current.ResolveRef(this, "AcrylicColor_Settings"), Interop.User32.AccentFlags.DrawAllBorders);
        }
    }
}
