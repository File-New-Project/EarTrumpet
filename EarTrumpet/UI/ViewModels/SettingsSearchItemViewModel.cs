using System;

namespace EarTrumpet.UI.ViewModels
{
    class SettingsSearchItemViewModel
    {
        public string DisplayName { get; set; }
        public string Glyph { get; set; }
        public Action Invoke { get; set; }
        public string SearchText { get; set; }

        public override string ToString() => SearchText;
    }
}
