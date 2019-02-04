using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.UI.ViewModels
{
    class SettingsSearchBoxResultsItemViewModel
    {
        public string DisplayName { get; set; }
        public string Glyph { get; set; }
        public Action Invoke { get; set; }
    }
}
