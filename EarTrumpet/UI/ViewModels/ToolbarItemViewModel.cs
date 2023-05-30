using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class ToolbarItemViewModel : BindableBase
    {
        private string _glyphFontFamily = "Segoe MDL2 Assets";

        public string DisplayName { get; set; }
        public string Glyph { get; set; }
        public string Id { get; set; }
        public int GlyphFontSize { get; set; }
        public string GlyphFontFamily
        {
            get => _glyphFontFamily;
            set
            {
                _glyphFontFamily = value;
                RaisePropertyChanged(nameof(GlyphFontFamily));
            }
        }
        public ObservableCollection<ContextMenuItem> Menu { get; set; }
        public ICommand Command { get; set; }
    }
}
