using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows.Threading;

namespace EarTrumpet.UI.ViewModels
{
    public class DeviceCollectionViewModel : BindableBase
    {
        public static readonly string DefaultDeviceChangedProperty = "DefaultDeviceChangedProperty";

        public event EventHandler Ready;
        public event EventHandler<DeviceViewModel> DefaultChanged;
        public event PropertyChangedEventHandler DefaultDevicePropertyChanged;
        public event Action TrayPropertyChanged;

        public ObservableCollection<DeviceViewModel> AllDevices { get; private set; }
        public DeviceViewModel Default { get; private set; }

        private readonly IAudioDeviceManager _deviceManager;
        private readonly Timer _peakMeterTimer;
        private readonly AppSettings _settings;
        private readonly Dispatcher _currentDispatcher = Dispatcher.CurrentDispatcher;
        private bool _isFlyoutVisible;
        private bool _isFullWindowVisible;

        public DeviceCollectionViewModel(IAudioDeviceManager deviceManager, AppSettings settings)
        {
            AllDevices = new ObservableCollection<DeviceViewModel>();
            _settings = settings;
            _deviceManager = deviceManager;
            _deviceManager.DefaultChanged += DeviceManager_DefaultDeviceChanged;
            _deviceManager.Loaded += DeviceManager_Loaded;
            _deviceManager.Devices.CollectionChanged += Devices_CollectionChanged;
            Devices_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            _peakMeterTimer = new Timer(1000 / 30); // 30 fps
            _peakMeterTimer.AutoReset = true;
            _peakMeterTimer.Elapsed += PeakMeterTimer_Elapsed;
        }

        private void DeviceManager_Loaded(object sender, EventArgs e)
        {
            Trace.WriteLine("DeviceCollectionViewModel DeviceManager_Loaded");
            Ready?.Invoke(this, null);
        }

        private void DeviceManager_DefaultDeviceChanged(object sender, IAudioDevice e)
        {
            if (e == null)
            {
                SetDefault(null);
            }
            else
            {
                var dev = AllDevices.FirstOrDefault(d => d.Id == e.Id);
                if (dev == null)
                {
                    AddDevice(e);
                    dev = AllDevices.FirstOrDefault(d => d.Id == e.Id);
                }
                SetDefault(dev);
            }
        }

        private void SetDefault(DeviceViewModel dev)
        {
            if (Default != null)
            {
                Default.PropertyChanged -= OnDefaultDevicePropertyChanged;
            }

            Default = dev;
            DefaultChanged?.Invoke(this, Default);

            if (Default != null)
            {
                Default.PropertyChanged += OnDefaultDevicePropertyChanged;
            }

            // Let clients know that even though no properties changed, the underlying object changed.
            OnDefaultDevicePropertyChanged(this, new PropertyChangedEventArgs(DefaultDeviceChangedProperty));
        }

        private void OnDefaultDevicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DefaultDevicePropertyChanged?.Invoke(sender, e);

            if (e.PropertyName == DefaultDeviceChangedProperty ||
                e.PropertyName == nameof(Default.Volume) ||
                e.PropertyName == nameof(Default.IsMuted) ||
                e.PropertyName == nameof(Default.DisplayName))
            {
                TrayPropertyChanged.Invoke();
            }
        }

        protected virtual void AddDevice(IAudioDevice device)
        {
            var newDevice = new DeviceViewModel(this, _deviceManager, device);
            AllDevices.Add(newDevice);
        }

        private void Devices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var added = ((IAudioDevice)e.NewItems[0]);
                    var allExistingAdded = AllDevices.FirstOrDefault(d => d.Id == added.Id);
                    if (allExistingAdded == null)
                    {
                        AddDevice(added);
                    }
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
                    foreach (var device in _deviceManager.Devices)
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
            _deviceManager.UpdatePeakValues();

            _currentDispatcher.BeginInvoke((Action)(() =>
            {
                foreach (var device in AllDevices)
                {
                    device.UpdatePeakValueForeground();
                }
            }));
        }

        public void MoveAppToDevice(IAppItemViewModel app, DeviceViewModel dev)
        {
            // Collect all matching apps on all devices.
            var apps = new List<IAppItemViewModel>();
            apps.Add(app);

            foreach (var device in AllDevices)
            {
                foreach (var deviceApp in device.Apps)
                {
                    if (deviceApp.DoesGroupWith(app))
                    {
                        if (!apps.Contains(deviceApp))
                        {
                            apps.Add(deviceApp);
                            break;
                        }
                    }
                }
            }

            foreach (var foundApp in apps)
            {
                MoveAppToDeviceInternal(foundApp, dev);
            }

            // Collect and move any hidden/moved sessions.
            ((IAudioDeviceManagerWindowsAudio)_deviceManager).MoveHiddenAppsToDevice(app.AppId, dev?.Id);
        }

        private void MoveAppToDeviceInternal(IAppItemViewModel app, DeviceViewModel dev)
        {
            var searchId = dev?.Id;
            if (dev == null)
            {
                searchId = _deviceManager.Default.Id;
            }

            try
            {
                DeviceViewModel oldDevice = AllDevices.First(d => d.Apps.Contains(app));
                DeviceViewModel newDevice = AllDevices.First(d => searchId == d.Id);

                bool isLogicallyMovingDevices = (oldDevice != newDevice);

                var tempApp = new TemporaryAppItemViewModel(this, _deviceManager, app);

                app.MoveToDevice(dev?.Id, hide: isLogicallyMovingDevices);

                // Update the UI if the device logically changed places.
                if (isLogicallyMovingDevices)
                {
                    oldDevice.AppLeavingFromThisDevice(app);
                    newDevice.AppMovingToThisDevice(tempApp);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{ex}");
            }
        }

        private void StartOrStopPeakTimer()
        {
            _peakMeterTimer.Enabled = _isFlyoutVisible || _isFullWindowVisible;
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

        public string GetTrayToolTip()
        {
            if (Default != null)
            {
                var stateText = Default.IsMuted ? Properties.Resources.MutedText : $"{Default.Volume}%";
                var prefixText = $"EarTrumpet: {stateText} - ";
                var deviceName = $"{Default.DeviceDescription} ({Default.EnumeratorName})";

                // Note: Remote Desktop has an empty description and empty enumerator, but the friendly name is set.
                if (string.IsNullOrWhiteSpace(Default.DeviceDescription) && string.IsNullOrWhiteSpace(Default.EnumeratorName))
                {
                    deviceName = Default.DisplayName;
                }

                // Device name could be null in transient error cases
                if (deviceName == null)
                {
                    deviceName = "";
                }

                // API Limitation: "less than 64 chars" for the tooltip.
                deviceName = deviceName.Substring(0, Math.Min(63 - prefixText.Length, deviceName.Length));
                return prefixText + deviceName;
            }
            else
            {
                return Properties.Resources.NoDeviceTrayText;
            }
        }
    }
}