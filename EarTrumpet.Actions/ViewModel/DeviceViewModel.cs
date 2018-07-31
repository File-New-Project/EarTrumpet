using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet_Actions.ViewModel
{
    class DeviceViewModel : BindableBase
    {
        public ObservableCollection<Option> All { get; }

        public Option Selected
        {
            get => All.FirstOrDefault(d => ((Device)d.Value)?.Id == _part.Device?.Id);
            set
            {
                if (Selected != value)
                {
                    _part.Device = (Device)value.Value;
                    RaisePropertyChanged(nameof(Selected));
                }
            }
        }

        private IPartWithDevice _part;

        public DeviceViewModel(IPartWithDevice part, Device.DeviceListKind flags)
        {
            _part = part;
            All = Device.GetDevices(flags);
            if (Selected == null && _part.Device?.Id != null)
            {
                All.Add(new Option(_part.Device.Id, _part.Device.Id));
            }
            
            if (_part.Device?.Id == null)
            {
                _part.Device = (Device)All[0].Value;
            }
        }
    }
}
