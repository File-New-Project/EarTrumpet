using EarTrumpet.Interop.Helpers;
using System;
using System.Diagnostics;

namespace EarTrumpet.UI.Services
{
    class HotkeyService
    {
        public static event EventHandler<KeyboardHook.KeyPressedEventArgs> KeyPressed;

        private static KeyboardHook s_hook;

        public static void Register(SettingsService.HotkeyData hotkey)
        {
            Trace.WriteLine($"HotkeyService Register {hotkey}");
            if (s_hook != null)
            {
                s_hook.Dispose();
            }

            s_hook = new KeyboardHook();
            s_hook.KeyPressed += Hotkey_KeyPressed;

            try
            {
                s_hook.RegisterHotKey(hotkey.Key, hotkey.Modifiers);
            }
            catch(Exception ex)
            {
                Trace.TraceError($"Couldn't register hotkey: {ex}");
            }
        }

        private static void Hotkey_KeyPressed(object sender, KeyboardHook.KeyPressedEventArgs e)
        {
            Trace.WriteLine($"HotkeyService Hotkey_KeyPressed");
            KeyPressed?.Invoke(s_hook, e);
        }

        public static void Unregister()
        {
            Trace.WriteLine($"HotkeyService Unregister");

            if (s_hook != null)
            {
                s_hook.Dispose();
            }
        }
    }
}
