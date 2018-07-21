using EarTrumpet.DataModel;
using EarTrumpet.Extensibility;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Services;
using System.Collections.Generic;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class AddonSettingsWindow : Window, ISettingsWindowHost
    {
        private static Dictionary<ISettingsEntry, AddonSettingsWindow> s_windows = new Dictionary<ISettingsEntry, AddonSettingsWindow>();

        private readonly ISettingsEntry _addon;
        public AddonSettingsWindow(ISettingsEntry addon)
        {
            _addon = addon;
            InitializeComponent();

            Title = addon.DisplayName;

            addon.Advise(this);
            AddonHostGrid.Children.Add((UIElement)addon.Content);

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

        public static void ShowForAddon(ISettingsEntry addon)
        {
            if (s_windows.ContainsKey(addon))
            {
                s_windows[addon].RaiseWindow();
            }
            else
            {
                var win = new AddonSettingsWindow(addon);
                win.Show();
                s_windows.Add(addon, win);
            }
        }

        public HotkeyData GetHotkeyFromUser()
        {
            var win = new HotkeySelectionWindow();
            win.Owner = this;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if ((bool)win.ShowDialog())
            {
                return win.Hotkey;
            }
            return null;
        }
    }
}
