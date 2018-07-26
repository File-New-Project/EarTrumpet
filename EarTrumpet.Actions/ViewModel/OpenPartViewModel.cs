using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EarTrumpet_Actions.ViewModel
{
    class OpenPartViewModel : BindableBase
    {
        public ICommand UnselectPart { get; set; }

        public PartViewModel Part { get; set; }

    }
}
