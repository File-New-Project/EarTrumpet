using System;
using System.Windows.Forms;

namespace EarTrumpet.Interop.Helpers
{
    public class HotkeyData
    {
        [Flags]
        public enum ModifierKeys : uint
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }

        public Keys Modifiers { get; set; }
        public Keys Key { get; set; }

        public HotkeyData(Message msg)
        {
            Key = (Keys)(((int)msg.LParam >> 16) & 0xFFFF);
            Modifiers = ModifiersToKeys((ModifierKeys)((int)msg.LParam & 0xFFFF));
        }

        public HotkeyData() { }

        public override string ToString()
        {
            return (string)(new KeysConverter()).ConvertTo(Modifiers | Key, typeof(string));
        }

        public override bool Equals(object obj)
        {
            var other = (HotkeyData)obj;
            return other.Key == Key && other.Modifiers == Modifiers;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public bool IsEmpty => Key == Keys.None && Modifiers == Keys.None;


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

        public uint GetInteropModifiers()
        {
            return (uint)KeysToModifiers(Modifiers);
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
