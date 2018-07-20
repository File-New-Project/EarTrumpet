using EarTrumpet.DataModel;
using EarTrumpet.Extensibility;
using EarTrumpet.Interop.Helpers;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class AddonSettingsWindow : Window
    {
        private readonly IHaveSettings _addon;
        public AddonSettingsWindow(IHaveSettings addon)
        {
            _addon = addon;
            InitializeComponent();

            Title = $"EarTrumpet - {addon.DisplayName}";

            AddonHostGrid.Children.Add((UIElement)addon.Content);

            SourceInitialized += (_, __) => AccentPolicyLibrary.SetWindowBlur(this, true, true);

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void ShowForAddon(IHaveSettings addon)
        {
            var win = new AddonSettingsWindow(addon);
            win.Show();

            // TODO: dictionary and single-instance behavior
        }
    }
}
