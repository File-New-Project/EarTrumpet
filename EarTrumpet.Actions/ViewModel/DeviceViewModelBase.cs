using EarTrumpet.DataModel;
using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.Actions.ViewModel
{
    public class DeviceViewModelBase : BindableBase
    {
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
        public string Id { get; set; }
        public AudioDeviceKind Kind { get; set; }

    }
}
