using EarTrumpet.UI.ViewModels;
using System.Windows.Input;

namespace EarTrumpet_Actions.ViewModel
{
    class OpenPartViewModel : BindableBase
    {
        public string Title => "Modify Part";

        public ICommand UnselectPart { get; set; }

        public PartViewModel Part { get; set; }

    }
}
