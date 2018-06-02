using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace EarTrumpet.ViewModels
{
    public class MainViewModel : BindableBase
    {
        public static MainViewModel Instance { get; private set; }

        public event EventHandler FlyoutShowRequested;

        public ObservableCollection<DeviceViewModel> AllDevices { get; private set; }

        private readonly IAudioDeviceManager _deviceService;
        private readonly Timer _peakMeterTimer;
        private bool _isFlyoutVisible;
        private bool _isFullWindowVisible;

        internal MainViewModel(IAudioDeviceManager deviceService)
        {
            Debug.Assert(Instance == null);
            Instance = this;

            AllDevices = new ObservableCollection<DeviceViewModel>();

            _deviceService = deviceService;
            _deviceService.Devices.CollectionChanged += Devices_CollectionChanged;
            Devices_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            _peakMeterTimer = new Timer(1000 / 30); // 30 fps
            _peakMeterTimer.AutoReset = true;
            _peakMeterTimer.Elapsed += PeakMeterTimer_Elapsed;
        }

        private void AddDevice(IAudioDevice device)
        {
            var newDevice = new DeviceViewModel(_deviceService, device);
            AllDevices.Add(newDevice);
        }

        private void Devices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddDevice((IAudioDevice)e.NewItems[0]);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var removed = ((IAudioDevice)e.OldItems[0]).Id;
                    var allExisting = AllDevices.FirstOrDefault(d => d.Id == removed);
                    if (allExisting != null)
                    {
                        AllDevices.Remove(allExisting);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    AllDevices.Clear();
                    foreach (var device in _deviceService.Devices)
                    {
                        AddDevice(device);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void PeakMeterTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // We're in the background so we need to use a snapshot.
            foreach (var device in AllDevices.ToArray())
            {
                device.UpdatePeakValueBackground();
            }

            App.Current.Dispatcher.SafeInvoke(() =>
            {
                foreach (var device in AllDevices)
                {
                    device.UpdatePeakValueForeground();
                }
            });
        }

        public void MoveAppToDevice(AppItemViewModel app, DeviceViewModel dev)
        {
            var searchId = dev?.Id;
            if (dev == null)
            {
                searchId = _deviceService.DefaultPlaybackDevice.Id;
            }
            DeviceViewModel oldDevice = AllDevices.First(d => d.Apps.Contains(app));
            DeviceViewModel newDevice = AllDevices.First(d => searchId == d.Id);

            try
            {
                app.MoveAllSessionsToDevice(dev?.Id);

                // Update the UI if the device logically changed places.
                if (oldDevice != newDevice)
                {
                    newDevice.OnAppMovedToDevice(app);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
            }
        }

        private void StartOrStopPeakTimer()
        {
            _peakMeterTimer.Enabled = (_isFlyoutVisible | _isFullWindowVisible);
        }

        public void OnTrayFlyoutShown()
        {
            _isFlyoutVisible = true;
            StartOrStopPeakTimer();
        }

        public void OnTrayFlyoutHidden()
        {
            _isFlyoutVisible = false;
            StartOrStopPeakTimer();
        }

        public void OnFullWindowClosed()
        {
            _isFullWindowVisible = false;
            StartOrStopPeakTimer();
        }

        public void OnFullWindowOpened()
        {
            _isFullWindowVisible = true;
            StartOrStopPeakTimer();
        }

        public void OpenFlyout()
        {
            Trace.WriteLine($"MainViewModel OpenFlyout");
            FlyoutShowRequested(this, null);
        }
    }
}
