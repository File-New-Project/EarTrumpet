using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using System;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class HotkeySelectViewModel : BindableBase, IWindowHostedViewModel
    {
        public event Action Close;
#pragma warning disable CS0067
        public event Action<object> HostDialog;
#pragma warning restore CS0067

        public ICommand Save { get; }

        public bool Saved { get; private set; }

        public string Title => Properties.Resources.SelectHotkeyWindowTitle;

        public string HotkeyText => Hotkey.ToString();

        public HotkeyData Hotkey { get; }

        public HotkeySelectViewModel()
        {
            Hotkey = new HotkeyData();
            HotkeyManager.Current.Pause();

            Save = new RelayCommand(() =>
            {
                Saved = true;
                Close();
            });
        }

        public void OnPreviewKeyDown(KeyEventArgs e)
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

        public void OnClosing()
        {
            HotkeyManager.Current.Resume();
        }
    }
}
