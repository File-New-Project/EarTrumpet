using System.Collections.Generic;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class ContextMenuItem
    {
        public string Glyph { get; set; } = "\xE0E7"; // Checkmark
        public string DisplayName { get; set; }
        public ICommand Command { get; set; }
        public bool IsChecked { get; set; }
        public IEnumerable<ContextMenuItem> Children { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public class ContextMenuSeparator : ContextMenuItem
    {
    }
}