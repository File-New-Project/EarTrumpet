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
            Hotkey = hotkey;
            InitializeComponent();

            UpdateText();

            SourceInitialized += (_, __) => this.SetWindowBlur(true, true);
        }

        void UpdateText()
        {
            HotkeyText.Text = Hotkey.ToString();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Hotkey.Modifiers = KeyboardHook.ModifierKeys.None;
            Hotkey.Key = System.Windows.Forms.Keys.None;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Hotkey.Modifiers = KeyboardHook.ModifierKeys.Control;
            }

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Hotkey.Modifiers |= KeyboardHook.ModifierKeys.Shift;
            }

            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                Hotkey.Modifiers |= KeyboardHook.ModifierKeys.Alt;
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
                if (Hotkey.Modifiers > 0)
                {
                    Hotkey.Key = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
                }
            }

            UpdateText();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
