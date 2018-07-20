using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EarTrumpet.Interop.Helpers
{
    public sealed class KeyboardHook : IDisposable
    {
        public class KeyPressedEventArgs : EventArgs
        {
            public Keys Modifiers;
            public Keys Key;
        }

        [Flags]
        enum ModifierKeys : uint
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }

        private class Window : NativeWindow, IDisposable
        {
            private static readonly int WM_HOTKEY = 0x0312;

            public void Initialize()
            {
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == WM_HOTKEY)
                {
                    KeyPressed?.Invoke(this, new KeyPressedEventArgs
                    {
                        Key = (Keys)(((int)m.LParam >> 16) & 0xFFFF),
                        Modifiers = ModifiersToKeys((ModifierKeys)((int)m.LParam & 0xFFFF))
                    });
                }
            }

            public event EventHandler<KeyPressedEventArgs> KeyPressed;

            public void Dispose()
            {
                this.DestroyHandle();
            }
        }

        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        private Window _window;
        private int _lastId = 0;

        public KeyboardHook()
        {
            _window = new Window();
            _window.Initialize();
            _window.KeyPressed += (s, e) => KeyPressed?.Invoke(this, e);
        }

        public void RegisterHotKey(Keys key, Keys modifiers)
        {
            if (!User32.RegisterHotKey(_window.Handle, ++_lastId, (uint)KeysToModifiers(modifiers), (uint)key))
            {
                throw new Exception($"Couldn't register hotkey: LastError={Marshal.GetLastWin32Error()}");
            }
        }

        public void Dispose()
        {
            for (var i = 1; i <= _lastId; i++)
            {
                User32.UnregisterHotKey(_window.Handle, i);
            }

            _window.Dispose();
        }

        private static Keys ModifiersToKeys(ModifierKeys modifiers)
        {
            Keys ret = Keys.None;
            if ((modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ret |= Keys.Control;
            }
            if ((modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                ret |= Keys.Alt;
            }
            if ((modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                ret |= Keys.Shift;
            }
            return ret;
        }

        private static ModifierKeys KeysToModifiers(Keys modifiers)
        {
            ModifierKeys ret = ModifierKeys.None;
            if ((modifiers & Keys.Control) == Keys.Control)
            {
                ret |= ModifierKeys.Control;
            }
            if ((modifiers & Keys.Alt) == Keys.Alt)
            {
                ret |= ModifierKeys.Alt;
            }
            if ((modifiers & Keys.Shift) == Keys.Shift)
            {
                ret |= ModifierKeys.Shift;
            }
            return ret;
        }
    }
}