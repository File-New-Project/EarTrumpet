using EarTrumpet.Interop;
using System;
using System.Windows.Forms;

namespace EarTrumpet.Misc
{
    public sealed class KeyboardHook : IDisposable
    {
        public class KeyPressedEventArgs : EventArgs
        {
            public ModifierKeys Modifier { get; }
            public Keys Key { get; }

            internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
            {
                Modifier = modifier;
                Key = key;
            }
        }

        [Flags]
        public enum ModifierKeys : uint
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

            public Window()
            {
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == WM_HOTKEY)
                {
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                    KeyPressed?.Invoke(this, new KeyPressedEventArgs(modifier, key));
                }
            }

            public event EventHandler<KeyPressedEventArgs> KeyPressed;

            public void Dispose()
            {
                this.DestroyHandle();
            }
        }

        private Window _window = new Window();
        private int _currentId;

        public KeyboardHook()
        {
            _window.KeyPressed += (s, e) => KeyPressed?.Invoke(this, e);
        }

        public void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            _currentId = _currentId + 1;

            if (!User32.RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
                throw new Exception("Couldn't register hotkey.");
        }

        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        public void Dispose()
        {
            for (int i = _currentId; i > 0; i--)
            {
                User32.UnregisterHotKey(_window.Handle, i);
            }

            _window.Dispose();
        }
    }
}
