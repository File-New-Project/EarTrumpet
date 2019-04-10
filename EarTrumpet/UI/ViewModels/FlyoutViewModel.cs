﻿using EarTrumpet.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace EarTrumpet.UI.ViewModels
{
    public class FlyoutViewModel : BindableBase, IPopupHostViewModel
    {
        public enum ViewState
        {
            NotLoaded,
            Hidden,
            Opening,
            Open,
            Closing_Stage1, // Closing animation (optional)
            Closing_Stage2, // Delay stage (optional)
        }

        public event EventHandler<object> WindowSizeInvalidated = delegate { };
        public event EventHandler<object> StateChanged = delegate { };

        public ModalDialogViewModel Dialog { get; }
        public bool IsExpanded { get; private set; }
        public bool CanExpand => _mainViewModel.AllDevices.Count > 1;
        public string DeviceNameText => Devices.Count > 0 ? Devices[0].DisplayName : null;
        public ViewState State { get; private set; }
        public ObservableCollection<DeviceViewModel> Devices { get; private set; }
        public RelayCommand ExpandCollapse { get; set; }
        public FlyoutShowOptions ShowOptions { get; private set; }

        private readonly DeviceCollectionViewModel _mainViewModel;
        private readonly DispatcherTimer _hideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        private bool _closedDuringOpen;

        public FlyoutViewModel(DeviceCollectionViewModel mainViewModel)
        {
            Dialog = new ModalDialogViewModel();
            Devices = new ObservableCollection<DeviceViewModel>();

            _mainViewModel = mainViewModel;
            _mainViewModel.DefaultChanged += OnDefaultPlaybackDeviceChanged;
            _mainViewModel.AllDevices.CollectionChanged += AllDevices_CollectionChanged;
            AllDevices_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            _hideTimer.Tick += HideTimer_Tick;
        }

        private void HideTimer_Tick(object sender, EventArgs e)
        {
            Debug.Assert(State == ViewState.Closing_Stage2);
            _hideTimer.IsEnabled = false;
            ChangeState(ViewState.Hidden);
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
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    if (Dialog.Focused is FocusedAppItemViewModel &&
                        (IAppItemViewModel)e.OldItems[0] == ((FocusedAppItemViewModel)Dialog.Focused)?.App)
                    {
                        Dialog.IsVisible = false;
                    }
                    break;
            }

            InvalidateWindowSize();
        }

        private void RemoveDevice(string id)
        {
            var existing = Devices.FirstOrDefault(d => d.Id == id);
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
                    RemoveDevice(((DeviceViewModel)e.OldItems[0]).Id);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    for (int i = Devices.Count - 1; i >= 0; i--)
                    {
                        RemoveDevice(Devices[i].Id);
                    }

                    foreach (var device in _mainViewModel.AllDevices)
                    {
                        AddDevice(device);
                    }

                    OnDefaultPlaybackDeviceChanged(null, _mainViewModel.Default);
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (!CanExpand)
            {
                IsExpanded = false;
            }

            UpdateTextVisibility();
            RaiseDevicesChanged();
        }

        private void RaiseDevicesChanged()
        {
            RaisePropertyChanged(nameof(IsExpanded));
            RaisePropertyChanged(nameof(CanExpand));
            RaisePropertyChanged(nameof(DeviceNameText));
            InvalidateWindowSize();
        }

        private void OnDefaultPlaybackDeviceChanged(object sender, DeviceViewModel e)
        {
            // no longer any device
            if (e == null) return;

            var foundDevice = Devices.FirstOrDefault(d => d.Id == e.Id);
            if (foundDevice != null)
            {
                // Move to bottom.
                Devices.Move(Devices.IndexOf(foundDevice), Devices.Count - 1);
            }
            else
            {
                var foundAllDevice = _mainViewModel.AllDevices.FirstOrDefault(d => d.Id == e.Id);
                if (foundAllDevice != null)
                {
                    Devices.Clear();
                    foundAllDevice.Apps.CollectionChanged += Apps_CollectionChanged;
                    Devices.Add(foundAllDevice);
                }
            }
            UpdateTextVisibility();
            RaiseDevicesChanged();
        }

        private void UpdateTextVisibility()
        {
            for (var i = 0; i < Devices.Count; i++)
            {
                Devices[i].IsDisplayNameVisible = i > 0;
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
                        device.Apps.CollectionChanged += Apps_CollectionChanged;
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

                    if (device.Id != _mainViewModel.Default?.Id)
                    {
                        device.Apps.CollectionChanged -= Apps_CollectionChanged;
                        Devices.Remove(device);
                    }
                }
            }

            UpdateTextVisibility();
            RaiseDevicesChanged();
        }

        private void InvalidateWindowSize()
        {
            // We must be async because otherwise SetWindowPos will pump messages before the UI has updated.
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                WindowSizeInvalidated?.Invoke(this, null);
            }));
        }

        public void ChangeState(ViewState state)
        {
            Trace.WriteLine($"FlyoutViewModel ChangeState {state}");
            var oldState = State;

            bool isValidStateTransition =
                (oldState == ViewState.NotLoaded && state == ViewState.Hidden) ||
                (oldState == ViewState.Hidden && state == ViewState.Opening) ||
                (oldState == ViewState.Opening && state == ViewState.Open) ||
                (oldState == ViewState.Open && state == ViewState.Closing_Stage1) ||
                (oldState == ViewState.Closing_Stage1 && state == ViewState.Closing_Stage2) ||
                (oldState == ViewState.Closing_Stage1 && state == ViewState.Hidden) ||
                (oldState == ViewState.Closing_Stage2 && state == ViewState.Hidden);

            Debug.Assert(isValidStateTransition);

            State = state;
            StateChanged(this, null);

            if (state == ViewState.Open)
            {
                _mainViewModel.OnTrayFlyoutShown();

                if (_closedDuringOpen)
                {
                    _closedDuringOpen = false;
                    BeginClose();
                }
            }
            else if (state == ViewState.Closing_Stage1)
            {
                _mainViewModel.OnTrayFlyoutHidden();

                Dialog.IsVisible = false;
            }
            else if (state == ViewState.Closing_Stage2)
            {
                _hideTimer.Start();
            }
        }

        public void OpenPopup(object vm, FrameworkElement container)
        {
            Dialog.IsVisible = false;

            if (vm is IAppItemViewModel)
            {
                Dialog.Focused = new FocusedAppItemViewModel(_mainViewModel, (IAppItemViewModel)vm);
            }
            else if (vm is DeviceViewModel)
            {
                var deviceViewModel = new FocusedDeviceViewModel(_mainViewModel, (DeviceViewModel)vm);
                if (deviceViewModel.IsApplicable)
                {
                    Dialog.Focused = deviceViewModel;
                }
            }
            else
            {
                Dialog.Focused = (IFocusedViewModel)vm;
            }

            if (Dialog.Focused != null)
            {
                Dialog.Focused.RequestClose += () => Dialog.IsVisible = false;
                Dialog.Source = container;
                Dialog.IsVisible = true;
            }
        }

        public void BeginOpen()
        {
            if (State == ViewState.Hidden)
            {
                ChangeState(ViewState.Opening);
            }
        }

        public void BeginClose()
        {
            if (State == ViewState.Open)
            {
                ChangeState(ViewState.Closing_Stage1);
            }
            else if (State == ViewState.Opening)
            {
                _closedDuringOpen = true;
            }
        }

        public void OpenFlyout(FlyoutShowOptions options)
        {
            ShowOptions = options;

            if (State == ViewState.Closing_Stage2)
            {
                return;
            }

            switch (State)
            {
                case ViewState.Hidden:
                    BeginOpen();
                    break;
                case ViewState.Open:
                    BeginClose();
                    break;
            }
        }
    }
}
