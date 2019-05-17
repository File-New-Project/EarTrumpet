using EarTrumpet.DataModel;
using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace EarTrumpet.UI.ViewModels
{
    // This ViewModel is used in redirection scenarios. When we move an app to a device,
    // this serves as the visualziation and data container for that app until a real session is created.
    public class TemporaryAppItemViewModel : BindableBase, IAppItemViewModel
    {
        public event EventHandler Expired;

        public string Id { get; }
        public bool IsMuted
        {
            get => ChildApps != null ? ChildApps[0].IsMuted : _isMuted;
            set
            {
                if (ChildApps != null)
                {
                    ChildApps[0].IsMuted = value;
                }
                else
                {
                    _isMuted = value;
                    RaisePropertyChanged(nameof(IsMuted));
                }
            }
        }
        public int Volume
        {
            get => ChildApps != null ? ChildApps[0].Volume : _volume;
            set
            {
                if (ChildApps != null)
                {
                    ChildApps[0].Volume = value;
                }
                else
                {
                    _volume = value;
                    RaisePropertyChanged(nameof(Volume));
                }
            }
        }
        public Color Background { get; }
        public ObservableCollection<IAppItemViewModel> ChildApps { get; }
        public string DisplayName { get; }
        public string ExeName { get; }
        public string AppId { get; }
        public IconLoadInfo Icon { get; }
        public char IconText { get; }
        public bool IsExpanded { get; }
        public bool IsMovable { get; }
        public float PeakValue1 { get; }
        public float PeakValue2 { get; }
        public string PersistedOutputDevice => ((IAudioDeviceManagerWindowsAudio)_deviceManager).GetDefaultEndPoint(ProcessId);
        public int ProcessId { get; }
        public IDeviceViewModel Parent { get; }

        private int[] _processIds;
        private int _volume;
        private bool _isMuted;
        private IAudioDeviceManager _deviceManager;
        private WeakReference<DeviceCollectionViewModel> _parent;

        internal TemporaryAppItemViewModel(DeviceCollectionViewModel parent, IAudioDeviceManager deviceManager, IAppItemViewModel app, bool isChild = false)
        {
            _parent = new WeakReference<DeviceCollectionViewModel>(parent);
            if (!isChild)
            {
                ChildApps = new ObservableCollection<IAppItemViewModel>();
                foreach(var childApp in app.ChildApps)
                {
                    var vm = new TemporaryAppItemViewModel(parent, deviceManager, childApp, true);
                    vm.PropertyChanged += ChildApp_PropertyChanged;
                    ChildApps.Add(vm);
                }
            }

            _deviceManager = deviceManager;
            Id = app.Id;
            _isMuted = app.IsMuted;
            _volume = app.Volume;
            Background = app.Background;
            DisplayName = app.DisplayName;
            ExeName = app.ExeName;
            AppId = app.AppId;
            Icon = app.Icon;
            IconText = app.IconText;
            IsMovable = app.IsMovable;
            IsExpanded = isChild;
            PeakValue1 = 0;
            PeakValue2 = 0;
            ProcessId = app.ProcessId;
            Parent = app.Parent;

            if (ChildApps != null)
            {
                _processIds = ChildApps.Select(a => a.ProcessId).ToSet().ToArray();
            }
            else
            {
                _processIds = new int[] { ProcessId };
            }

            foreach(var pid in _processIds)
            {
                ProcessWatcherService.WatchProcess(pid, (pidQuit) =>
                {
                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        var newPids = _processIds.ToList();

                        if (newPids.Contains(pidQuit))
                        {
                            newPids.Remove(pidQuit);
                        }
                        _processIds = newPids.ToArray();

                        if (_processIds.Length == 0)
                        {
                            Expire();
                        }
                    }));
                });
            }

#if VSDEBUG
            Background = Colors.Red;
#endif
        }

        private void ChildApp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        public bool DoesGroupWith(IAppItemViewModel app)
        {
            return ExeName == app.ExeName;
        }

        public void MoveToDevice(string id, bool hide)
        {
            // Update the output for all processes represented by this app.
            foreach (var pid in _processIds)
            {
                ((IAudioDeviceManagerWindowsAudio)_deviceManager).SetDefaultEndPoint(id, pid);
            }

            if (hide)
            {
                Expire();
            }
        }

        private void Expire()
        {
            Expired?.Invoke(this, null);
        }

        public void UpdatePeakValueBackground() { }
        public void UpdatePeakValueForeground() { }
    }
}
