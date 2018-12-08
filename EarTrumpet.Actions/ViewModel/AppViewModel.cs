using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace EarTrumpet_Actions.ViewModel
{
    class AppViewModel : BindableBase, IOptionViewModel
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

        public AppViewModel(IPartWithApp part, App.AppKind flags)
        {
            _part = part;
            All = App.GetApps(flags);
            if (Selected == null && _part.App?.Id != null)
            {
                All.Add(new Option(_part.App.Id, _part.App));
            }

            if (_part.App?.Id == null)
            {
                _part.App = (App)All[0].Value;
            }
        }

        public override string ToString()
        {
            return Selected?.DisplayName;
        }
    }
}
