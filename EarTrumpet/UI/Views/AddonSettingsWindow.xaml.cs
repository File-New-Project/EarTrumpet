using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using System.Collections.Generic;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class AddonSettingsWindow : Window
    {
        private static Dictionary<object, AddonSettingsWindow> s_windows = new Dictionary<object, AddonSettingsWindow>();

        public AddonSettingsWindow(object addon, string displayName)
        {
            InitializeComponent();

            Title = displayName;

            AddonHostGrid.Children.Add((UIElement)addon);

            SourceInitialized += (_, __) => AccentPolicyLibrary.SetWindowBlur(this, true, true);

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            Closing += AddonSettingsWindow_Closing;
        }

        private void AddonSettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var pair in s_windows)
            {
                if (pair.Value == this)
                {
                    s_windows.Remove(pair.Key);
                    break;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void ShowForAddon(object addon, string displayName)
        {
            if (s_windows.ContainsKey(addon))
            {
                s_windows[addon].RaiseWindow();
            }
            else
            {
                var win = new AddonSettingsWindow(addon, displayName);
                win.Show();
                s_windows.Add(addon, win);
            }
        }
    }
}
