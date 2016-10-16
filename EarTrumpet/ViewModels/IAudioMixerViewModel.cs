using System.Collections.ObjectModel;
using System.Windows;

namespace EarTrumpet.ViewModels
{
    public interface IAudioMixerViewModel
    {
        ObservableCollection<AppItemViewModel> Apps { get; }
        DeviceAppItemViewModel Device { get; }
        Visibility DeviceVisibility { get; }
        Visibility ListVisibility { get; }
        Visibility NoAppsPaneVisibility { get; }
        string NoItemsContent { get; }

        void Refresh();
    }
}