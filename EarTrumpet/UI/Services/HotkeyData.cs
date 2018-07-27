using System.Windows.Forms;

namespace EarTrumpet.UI.Services
{
    public class HotkeyData
    {
        public Keys Modifiers;
        public Keys Key;

        public override string ToString()
        {
            return (string)(new KeysConverter()).ConvertTo(Modifiers | Key, typeof(string));
        }

        public bool IsEmpty => Key == Keys.None &&Modifiers == Keys.None;
    }
}
