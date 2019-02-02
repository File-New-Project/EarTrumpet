using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet_Actions.ViewModel
{
    public class ImportExportPageViewModel : SettingsPageViewModel
    {
        public ImportExportPageViewModel() : base("Management")
        {
            Title = "Import and export";
            Glyph = "\xE148";
        }
    }
}
