using System.Collections.ObjectModel;
using EarTrumpet_Actions.DataModel;

namespace EarTrumpet_Actions.ViewModel
{
    interface IOptionViewModel
    {
        ObservableCollection<Option> All { get; }
        Option Selected { get; set; }
    }
}