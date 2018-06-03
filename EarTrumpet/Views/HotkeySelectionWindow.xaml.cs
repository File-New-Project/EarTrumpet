using EarTrumpet.Extensions;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet.Views
{
    public partial class HotkeySelectionWindow : Window
    {
        public SettingsService.HotkeyData Hotkey { get; private set; }

        public HotkeySelectionWindow(SettingsService.HotkeyData hotkey)
        {
            Hotkey = new SettingsService.HotkeyData { Key = hotkey.Key, Modifiers = hotkey.Modifiers };
            InitializeComponent();

            UpdateText();

            SourceInitialized += (_, __) => this.SetWindowBlur(true, true);

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        void UpdateText()
        {
            HotkeyText.Text = Hotkey.ToString();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) return;
            if (e.Key == Key.Tab) return;

            Hotkey.Modifiers = System.Windows.Forms.Keys.None;
            Hotkey.Key = System.Windows.Forms.Keys.None;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Hotkey.Modifiers = System.Windows.Forms.Keys.Control;
            }

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Hotkey.Modifiers |= System.Windows.Forms.Keys.Shift;
            }

            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                Hotkey.Modifiers |= System.Windows.Forms.Keys.Alt;
            }

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.CapsLock || e.Key == Key.LWin || e.Key == Key.RWin)
            {
                // Ignore all types of modifiers
            }
            else
            {
                Hotkey.Key = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            }

            UpdateText();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }
    }
}
