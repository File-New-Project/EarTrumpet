using EarTrumpet_Actions.DataModel;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EarTrumpet_Actions.ViewModel
{
    class OptionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Option> All { get; }

        public Option Selected
        {
            get => _part.Options[_index].Selected;
            set
            {
                if (Selected != value)
                {
                    _part.Options[_index].Selected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((nameof(Selected))));
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
    }
}
