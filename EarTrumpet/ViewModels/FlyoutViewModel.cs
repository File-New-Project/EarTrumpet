using EarTrumpet.DataModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace EarTrumpet.ViewModels
{
    class FlyoutViewModel : BindableBase
    {
        public enum ViewState
        {
            NotReady,
            Hidden,
            Opening,
            Opening_CloseRequested,
            Open,
            Closing,
        }

        public event EventHandler<AppExpandedEventArgs> AppExpanded = delegate { };
        public event EventHandler<object> WindowSizeInvalidated = delegate { };
        public event EventHandler<object> AppCollapsed = delegate { };
        public event EventHandler<ViewState> StateChanged = delegate { };

        public bool IsExpanded { get; private set; }
        public bool CanExpand => _mainViewModel.AllDevices.Count > 1;
        public bool IsEmpty => Devices.Count == 0;
        public string ExpandText => CanExpand ? (IsExpanded ? "\ue011" : "\ue010") : "";
        public ViewState State { get; private set; }
        public bool IsShowingModalDialog { get; private set; }
        public ObservableCollection<DeviceViewModel> Devices { get; private set; }

        private IAudioDeviceManager _deviceManager;
        private MainViewModel _mainViewModel;

        public FlyoutViewModel(MainViewModel mainViewModel, IAudioDeviceManager deviceManager)
        {
            Devices = new ObservableCollection<DeviceViewModel>();

            _deviceManager = deviceManager;
            _mainViewModel = mainViewModel;

            _deviceManager.DefaultPlaybackDeviceChanged += OnDefaultPlaybackDeviceChanged;

            _mainViewModel.AllDevices.CollectionChanged += AllDevices_CollectionChanged;

            AllDevices_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void AddDevice(DeviceViewModel device)
        {
            if (IsExpanded || Devices.Count == 0)
            {
                device.Apps.CollectionChanged += Apps_CollectionChanged;
                Devices.Insert(0, device);
            }
        }

        private void Apps_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateWindowSize();
        }

        private void RemoveDevice(string id)
        {
            var existing = Devices.FirstOrDefault(d => d.Device.Id == id);
            if (existing != null)
            {
                existing.Apps.CollectionChanged -= Apps_CollectionChanged;
                Devices.Remove(existing);
            }
        }

        private void AllDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddDevice((DeviceViewModel)e.NewItems[0]);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveDevice(((DeviceViewModel)e.OldItems[0]).Device.Id);
                    break;

                case NotifyCollectionChangedAction.Reset:

                    for (int i = Devices.Count - 1; i >= 0; i--)
                    {
                        RemoveDevice(Devices[i].Device.Id);
                    }

                    foreach (var device in _mainViewModel.AllDevices)
                    {
                        AddDevice(device);
                    }

                    OnDefaultPlaybackDeviceChanged(null, _deviceManager.DefaultPlaybackDevice);
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (!CanExpand)
            {
                IsExpanded = false;
            }

            RaisePropertyChanged(nameof(IsEmpty));
            RaisePropertyChanged(nameof(CanExpand));
            RaisePropertyChanged(nameof(ExpandText));
            InvalidateWindowSize();
        }

        private void OnDefaultPlaybackDeviceChanged(object sender, IAudioDevice e)
        {
            // no longer any device
            if (e == null) return;

            var foundDevice = Devices.FirstOrDefault(d => d.Device.Id == e.Id);
            if (foundDevice != null)
            {
                // Move to bottom.
                Devices.Move(Devices.IndexOf(foundDevice), Devices.Count - 1);
            }
            else
            {
                var foundAllDevice = _mainViewModel.AllDevices.FirstOrDefault(d => d.Device.Id == e.Id);
                if (foundAllDevice != null)
                {
                    Devices.Clear();
                    Devices.Add(foundAllDevice);
                    InvalidateWindowSize();
                }
            }
        }

        public void DoExpandCollapse()
        {
            IsExpanded = !IsExpanded;

            if (IsExpanded)
            {
                // Add any that aren't existing.
                foreach (var device in _mainViewModel.AllDevices)
                {
                    if (!Devices.Contains(device))
                    {
                        Devices.Insert(0, device);
                    }
                }
            }
            else
            {
                // Remove all but default.
                for (int i = Devices.Count - 1; i >= 0; i--)
                {
                    var device = Devices[i];

                    if (device.Device.Id != _deviceManager.DefaultPlaybackDevice.Id)
                    {
                        Devices.Remove(device);
                    }
                }
            }

            RaisePropertyChanged(nameof(IsExpanded));
            RaisePropertyChanged(nameof(ExpandText));
            InvalidateWindowSize();
        }

        private void InvalidateWindowSize()
        {
            WindowSizeInvalidated?.Invoke(this, null);
        }

        public void ChangeState(ViewState state)
        {
            var oldState = State;

            State = state;
            StateChanged(this, state);

            if (state == ViewState.Open)
            {
                _mainViewModel.OnTrayFlyoutShown();

                if (oldState == ViewState.Opening_CloseRequested)
                {
                    BeginClose();
                }
            }
            else if (state == ViewState.Hidden)
            {
                _mainViewModel.OnTrayFlyoutHidden();

                if (IsShowingModalDialog)
                {
                    OnAppCollapsed();
                }
            }
        }

        public void OnAppExpanded(AppItemViewModel vm, UIElement container)
        {
            if (IsShowingModalDialog)
            {
                OnAppCollapsed();
            }

            AppExpanded?.Invoke(this, new AppExpandedEventArgs { Container = container, ViewModel = vm });

            IsShowingModalDialog = true;
            RaisePropertyChanged(nameof(IsShowingModalDialog));
        }

        public void OnAppCollapsed()
        {
            if (IsShowingModalDialog)
            {
                AppCollapsed?.Invoke(this, null);
                IsShowingModalDialog = false;
                RaisePropertyChanged(nameof(IsShowingModalDialog));
            }
        }

        public void BeginOpen()
        {
            if (State == ViewState.Hidden)
            {
                ChangeState(ViewState.Opening);
            }

            // NotReady - Ignore, can't do anything.
            // Opening - Ignore. Already opening.
            // Opening_CloseRequested - Ignore, already opening and then closing.
            // Open - We're already open.
            // Closing - Drop event. Not worth the complexity?
        }

        public void BeginClose()
        {
            if (State == ViewState.Open)
            {
                ChangeState(ViewState.Closing);
            }
            else if (State == ViewState.Opening)
            {
                ChangeState(ViewState.Opening_CloseRequested);
            }

            // NotReady - Impossible.
            // Hidden - Nothing to do.
            // Opening_CloseRequested - Nothing to do.
            // Closing - Nothing to do.
        }
    }
}
