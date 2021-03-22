using System.Collections.ObjectModel;

namespace EarTrumpet.Actions.ViewModel
{
    interface IOptionViewModel
    {
        ObservableCollection<Option> All { get; }
        Option Selected { get; set; }
    }
}