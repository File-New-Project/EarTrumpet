using System;

namespace EarTrumpet.UI.ViewModels
{
    class SettingsSearchBoxResultsItemViewModel
    {
        public string DisplayName { get; set; }
        public string Glyph { get; set; }
        public Action Invoke { get; set; }
    }
}
