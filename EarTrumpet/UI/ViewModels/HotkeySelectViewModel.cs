using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class HotkeySelectViewModel : BindableBase
    {
        public ICommand Save { get; set; }

        public string Title => Properties.Resources.SelectHotkeyWindowTitle;

        public string HotkeyText => Hotkey.ToString();

        public HotkeyData Hotkey { get; }

        public HotkeySelectViewModel()
        {
            Hotkey = new HotkeyData();
        }

        public void Window_PreviewKeyDown(object sender, KeyEventArgs e)
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

            RaisePropertyChanged(nameof(HotkeyText));
        }
    }
}
