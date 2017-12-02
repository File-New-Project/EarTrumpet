using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace EarTrumpet.ViewModels
{
    public class MainViewModel : BindableBase
    {
        public ObservableCollection<AppItemViewModel> Apps { get; private set; }
        public VolumeControlChannelViewModel Device { get; private set; }

        public Visibility ListVisibility { get; private set; }
        public Visibility NoAppsPaneVisibility { get; private set; }
        public Visibility DeviceVisibility { get; private set; }

        public string NoItemsContent { get; private set; }

        private readonly AudioDeviceManager _deviceService;

        public MainViewModel(AudioDeviceManager deviceService)
        {
            Apps = new ObservableCollection<AppItemViewModel>();

            _deviceService = deviceService;
            _deviceService.VirtualDefaultDevice.PropertyChanged += VirtualDefaultDevice_PropertyChanged;
            _deviceService.VirtualDefaultDevice.CollectionChanged += VirtualDefaultDevice_CollectionChanged;

            Device = new VolumeControlChannelViewModel(_deviceService.VirtualDefaultDevice);

            PopulateAppSessions();

            UpdateInterfaceState();
        }

        private void VirtualDefaultDevice_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    Apps.AddSorted(new AppItemViewModel((IAudioDeviceSession)e.NewItems[0]), AppItemViewModelComparer.Instance);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    Apps.Remove(Apps.First(x => x.Id == ((IAudioDeviceSession)e.OldItems[0]).Id));
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Apps.Clear();
                    PopulateAppSessions();
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
            }
        }

        internal void TriggerPeakCheck()
        {
            foreach(var app in Apps)
            {
                app.TriggerPeakCheck();
            }

            Device.TriggerPeakCheck();
        }

        private void PopulateAppSessions()
        {
            foreach(var session in _deviceService.VirtualDefaultDevice.Sessions.Sessions)
            {
                Apps.AddSorted(new AppItemViewModel(session), AppItemViewModelComparer.Instance);
            }
        }

        private void VirtualDefaultDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDevicePresent")
            {
                UpdateInterfaceState();
            }
        }

        public void UpdateInterfaceState()
        {
            ListVisibility = Apps.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            NoAppsPaneVisibility = Apps.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            NoItemsContent = !_deviceService.VirtualDefaultDevice.IsDevicePresent ? Properties.Resources.NoDevicesPanelContent : Properties.Resources.NoAppsPanelContent;
            DeviceVisibility = _deviceService.VirtualDefaultDevice.IsDevicePresent ? Visibility.Visible : Visibility.Collapsed;

            RaisePropertyChanged("ListVisibility");
            RaisePropertyChanged("NoAppsPaneVisibility");
            RaisePropertyChanged("NoItemsContent");
            RaisePropertyChanged("DeviceVisibility");
        }
    }
}
