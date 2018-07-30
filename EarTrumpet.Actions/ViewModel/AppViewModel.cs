using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace EarTrumpet_Actions.ViewModel
{
    class AppViewModel : BindableBase
    {
        public ObservableCollection<Option> All { get; }

        public Option Selected
        {
            get => All.FirstOrDefault(d => ((App)d.Value)?.Id == _part.App?.Id);
            set
            {
                if (Selected != value)
                {
                    _part.App = (App)value.Value;
                    RaisePropertyChanged(nameof(Selected));
                }
            }
        }

        private IPartWithApp _part;

        public AppViewModel(IPartWithApp part)
        {
            _part = part;
            All = App.AllApps;
            if (Selected == null)
            {
                All.Add(new Option(_part.App.Id, _part.App));
            }
        }
    }
}
