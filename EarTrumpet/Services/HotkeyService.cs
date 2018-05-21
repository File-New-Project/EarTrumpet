using EarTrumpet.Misc;
using System;
using System.Diagnostics;

namespace EarTrumpet.Services
{
    class HotkeyService
    {
        public static event EventHandler<KeyboardHook.KeyPressedEventArgs> KeyPressed;

        private static KeyboardHook s_hook;

        public static void Register(SettingsService.HotkeyData hotkey)
        {
            if (s_hook != null)
            {
                s_hook.Dispose();
            }

            s_hook = new KeyboardHook();
            s_hook.KeyPressed += Hotkey_KeyPressed;

            try
            {
                s_hook.RegisterHotKey(hotkey.Modifiers, hotkey.Key);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Couldn't register hotkey: {ex}");
            }
        }

        private static void Hotkey_KeyPressed(object sender, KeyboardHook.KeyPressedEventArgs e)
        {
            KeyPressed?.Invoke(s_hook, e);
        }

        public static void Unregister()
        {
            if (s_hook != null)
            {
                s_hook.Dispose();
            }
        }
    }
}
