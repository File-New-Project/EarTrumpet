using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class ToolbarItemViewModel
    {
        public string DisplayName { get; set; }
        public string Glyph { get; set; }
        public string Id { get; set; }
        public int GlyphFontSize { get; set; }
        public ObservableCollection<ContextMenuItem> Menu { get; set; }
        public ICommand Command { get; set; }
    }
}
