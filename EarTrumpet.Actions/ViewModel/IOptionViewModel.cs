using System.Collections.ObjectModel;
using EarTrumpet.Actions.DataModel;

namespace EarTrumpet.Actions.ViewModel
{
    interface IOptionViewModel
    {
        ObservableCollection<Option> All { get; }
        Option Selected { get; set; }
    }
}