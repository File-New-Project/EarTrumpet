using System.Windows.Media;

namespace EarTrumpet.UI
{
    public class IconLoadInfo
    {
        public bool IsDesktopApp { get; set; }
        public string IconPath { get; set; }
        public bool IsLoadComplete { get; set; }
        public ImageSource CachedValue { get; set; }
    }
}
