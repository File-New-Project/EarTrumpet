using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using System.Collections.ObjectModel;

namespace EarTrumpet_Actions.ViewModel
{
    class OptionViewModel : BindableBase
    {
        public ObservableCollection<Option> All { get; }

        public Option Selected
        {
            get => _part.Options[_index].Selected;
            set
            {
                if (Selected != value)
                {
                    _part.Options[_index].Selected = value;
                    RaisePropertyChanged(nameof(Selected));
                }
            }
        }

        private PartWithOptions _part;
        private int _index;

        public OptionViewModel(PartWithOptions part, int index = 0)
        {
            _index = index;
            _part = part;
            All = new ObservableCollection<Option>(part.Options[_index].Options);
        }

        public override string ToString()
        {
            return Selected?.DisplayName;
        }
    }
}
