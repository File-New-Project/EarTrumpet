using EarTrumpet_Actions.DataModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace EarTrumpet_Actions.ViewModel
{
    class AppViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Option> All { get; }

        public Option Selected
        {
            get => All.FirstOrDefault(d => ((App)d.Value)?.Id == _part.DeviceSession?.Id);
            set
            {
                if (Selected != value)
                {
                    _part.DeviceSession = (App)value.Value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((nameof(Selected))));
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
                All.Add(new Option(_part.DeviceSession.Id, _part.DeviceSession));
            }
        }
    }
}
