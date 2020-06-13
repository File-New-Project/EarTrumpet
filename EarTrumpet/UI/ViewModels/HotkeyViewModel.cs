using EarTrumpet.Interop.Helpers;
using System;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class HotkeyViewModel : BindableBase
    {
        private string _hotkeyText;
        public string HotkeyText
        {
            get => _hotkeyText;
            set
            {
                if (_hotkeyText != value)
                {
                    _hotkeyText = value;
                    RaisePropertyChanged(nameof(HotkeyText));

                    if (string.IsNullOrWhiteSpace(_hotkeyText))
                    {
                        _hotkey.Modifiers = System.Windows.Forms.Keys.None;
                        _hotkey.Key = System.Windows.Forms.Keys.None;
                    }
                }
            }
        }

        private readonly Action<HotkeyData> _save;
        private HotkeyData _hotkey;
        private HotkeyData _savedHotkey;

        public HotkeyViewModel(HotkeyData hotkey, Action<HotkeyData> save)
        {
            _hotkey = hotkey;
            _savedHotkey = new HotkeyData { Key = hotkey.Key, Modifiers = hotkey.Modifiers };
            _save = save;

            SetHotkeyText();
        }

        public void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // If Alt is down, a system key is being prepared
            var key = (e.Key == Key.System) ? e.SystemKey : e.Key;

            // Impossible hotkeys (even with modifiers):
            // Tab, Backspace, Escape
            if (key == Key.Tab)
            {
                return;
            }

            e.Handled = true;

            if (key == Key.Escape || key == Key.Back)
            {
                // Clear selection
                _hotkey.Key = System.Windows.Forms.Keys.None;
                _hotkey.Modifiers = System.Windows.Forms.Keys.None;
            }
            else
            {
                _hotkey.Modifiers = System.Windows.Forms.Keys.None;
                _hotkey.Key = System.Windows.Forms.Keys.None;

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    _hotkey.Modifiers = System.Windows.Forms.Keys.Control;
                }

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    _hotkey.Modifiers |= System.Windows.Forms.Keys.Shift;
                }

                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    _hotkey.Modifiers |= System.Windows.Forms.Keys.Alt;
                }

                if (key == Key.LeftShift || key == Key.RightShift ||
                    key == Key.LeftAlt || key == Key.RightAlt ||
                    key == Key.LeftCtrl || key == Key.RightCtrl ||
                    key == Key.CapsLock || key == Key.LWin || key == Key.RWin)
                {
                    // Ignore all types of modifiers
                }
                else
                {
                    _hotkey.Key = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(key);
                }
            }
            SetHotkeyText();
        }

        public void OnLostFocus(object sender, RoutedEventArgs e)
        {
            // Disallow e.g. Alt+None modifier-only hotkeys.
            if (_hotkey.Key == System.Windows.Forms.Keys.None &&
                _hotkey.Modifiers != System.Windows.Forms.Keys.None)
            {
                _hotkey.Modifiers = System.Windows.Forms.Keys.None;
                SetHotkeyText();
            }

            if (_hotkey != _savedHotkey)
            {
                _save(_hotkey);
                _savedHotkey = new HotkeyData { Key = _hotkey.Key, Modifiers = _hotkey.Modifiers };
            }
            HotkeyManager.Current.Resume();
        }

        public void OnGotFocus(object sender, RoutedEventArgs e)
        {
            HotkeyManager.Current.Pause();
        }

        private void SetHotkeyText()
        {
            HotkeyText = _hotkey.ToString().Replace(System.Windows.Forms.Keys.None.ToString(), "");
        }
    }
}
