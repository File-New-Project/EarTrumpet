using System;

namespace EarTrumpet.UI.ViewModels
{
    class SettingsSearchItemViewModel
    {
        public string DisplayName { get; set; }
        public string Glyph { get; set; }
        public Action Invoke { get; set; }
        public string SearchText { get; set; }

        // This is kind of a hack, having the item report as the search text
        // enables a ComboBoxItem to represent the item but also show in the 
        // TextBox as the text search query entered by the user.
        public override string ToString() => SearchText;
    }
}
