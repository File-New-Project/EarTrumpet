using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EarTrumpet
{
    public sealed class KeyboardHook : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private class Window : NativeWindow, IDisposable
        {
            private static int WM_HOTKEY = 0x0312;

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
                    KBModifierKeys modifier = (KBModifierKeys)((int)m.LParam & 0xFFFF);

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
            _window.KeyPressed += (s, e) =>
            {
                    KeyPressed?.Invoke(this, e);
            };
        }

        public void RegisterHotKey(KBModifierKeys modifier, Keys key)
        {
            _currentId = _currentId + 1;

            if (!RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
                throw new Exception("Couldn’t register hotkey.");
        }

        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        public void Dispose()
        {
            for (int i = _currentId; i > 0; i--)
            {
                UnregisterHotKey(_window.Handle, i);
            }

            _window.Dispose();
        }
    }

    public class KeyPressedEventArgs : EventArgs
    {
        private KBModifierKeys _modifier;
        private Keys _key;

        internal KeyPressedEventArgs(KBModifierKeys modifier, Keys key)
        {
            _modifier = modifier;
            _key = key;
        }

        public KBModifierKeys Modifier
        {
            get { return _modifier; }
        }

        public Keys Key
        {
            get { return _key; }
        }
    }

    [Flags]
    public enum KBModifierKeys : uint
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
