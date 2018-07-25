using EarTrumpet_Actions.DataModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace EarTrumpet_Actions.ViewModel
{
    class DeviceViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Option> All { get; }

        public Option Selected
        {
            get => All.First(d => ((Device)d.Value)?.Id == _part.Device?.Id);
            set
            {
                if (Selected != value)
                {
                    _part.Device = (Device)value.Value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((nameof(Selected))));
                }
            }
        }

        private IPartWithDevice _part;

        public DeviceViewModel(IPartWithDevice part)
        {
            _part = part;
            All = Device.AllDevices;
            if (Selected == null)
            {
                All.Add(new Option(_part.Device.Id, _part.Device.Id));
            }
        }
    }
}
