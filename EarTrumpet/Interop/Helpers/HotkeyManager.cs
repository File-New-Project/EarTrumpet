using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace EarTrumpet.Interop.Helpers
{
    public class HotkeyManager
    {
        private class Entry
        {
            public HotkeyData Hotkey;
            public int RefCount;
            public int Id;
        }

        public static HotkeyManager Current { get; } = new HotkeyManager();

        public event Action<HotkeyData> KeyPressed;

        private readonly Dictionary<HotkeyData, Entry> _data;
        private readonly Win32Window _window;
        private int _lastId;

        public HotkeyManager()
        {
            _data = new Dictionary<HotkeyData, Entry>();
            _window = new Win32Window();
            _window.Initialize(WndProc);
        }

        private void WndProc(Message msg)
        {
            if (msg.Msg == User32.WM_HOTKEY)
            {
                var hotkey = new HotkeyData(msg);
                Trace.WriteLine($"HotkeyManager: KeyPressed: {hotkey}");
                KeyPressed?.Invoke(hotkey);
            }
        }

        public void Register(HotkeyData hotkey)
        {
            if (hotkey.IsEmpty)
            {
                return;
            }

            Entry entry;
            if (_data.ContainsKey(hotkey))
            {
                entry = _data[hotkey];
            }
            else
            {
                entry = _data[hotkey] = new Entry { Id = ++_lastId, Hotkey = hotkey };
                User32.RegisterHotKey(_window.Handle, entry.Id, hotkey.GetInteropModifiers(), (uint)hotkey.Key);
            }

            entry.RefCount++;

            Trace.WriteLine($"HotkeyManager: Register: {hotkey}");
        }

        public void Unregister(HotkeyData hotkey)
        {
            if (hotkey.IsEmpty) return;

            var entry = _data[hotkey];
            entry.RefCount--;

            Trace.WriteLine($"HotkeyManager: Unregister: {hotkey} {entry.RefCount}");
            if (entry.RefCount == 0)
            {
                User32.UnregisterHotKey(_window.Handle, entry.Id);
                _data.Remove(hotkey);
            }

        }

        public void Pause()
        {
            Trace.WriteLine($"HotkeyManager: Pause");
            foreach (var entry in _data.Values)
            {
                User32.UnregisterHotKey(_window.Handle, entry.Id);
            }
        }

        public void Resume()
        {
            Trace.WriteLine($"HotkeyManager: Resume");
            foreach (var entry in _data.Values)
            {
                User32.RegisterHotKey(_window.Handle, entry.Id, entry.Hotkey.GetInteropModifiers(), (uint)entry.Hotkey.Key);
            }
        }
    }
}
