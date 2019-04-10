using EarTrumpet.UI.Tray;
using System.Drawing;

namespace EarTrumpet.Extensibility
{
    public class AddonTrayIconEventArgs
    {
        public IconKind Kind { get; set; }
        public Icon Icon { get; set; }
    }

    public interface IAddonTrayIcon
    {
        // Existing:
        //
        // ThemeColorBuiltIn: 20
        // LegacyIconBuiltIn: 30
        // HighContrastBuiltIn: 40
        // 
        // A good place for your addon: 100
        int Priority { get; }
        void TrayIconChanging(AddonTrayIconEventArgs e);
    }
}
