using EarTrumpet.Misc;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace EarTrumpet.Services
{
    class HotkeyService
    {
        public static event EventHandler<KeyPressedEventArgs> KeyPressed;

        static KeyboardHook s_hook;

        public static void Register(KBModifierKeys modifiers, Keys key)
        {
            if (s_hook != null)
            {
                s_hook.Dispose();
            }

            s_hook = new KeyboardHook();
            s_hook.KeyPressed += Hotkey_KeyPressed;

            try
            {
                s_hook.RegisterHotKey(modifiers, key);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Couldn't register hotkey: {ex}");
            }
        }

        static void Hotkey_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            KeyPressed?.Invoke(s_hook, e);
        }
    }
}
