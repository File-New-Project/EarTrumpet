﻿using EarTrumpet.DataModel;
using EarTrumpet.DataModel.Internal.Services;
using EarTrumpet.Extensions;
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
        public SolidColorBrush Background { get; }
        public ObservableCollection<IAppItemViewModel> ChildApps { get; }
        public string DisplayName { get; }
        public string ExeName { get; }
        public string AppId { get; }
        public ImageSource Icon { get; }
        public char IconText { get; }
        public bool IsExpanded { get; }
        public bool IsMovable { get; }
        public float PeakValue1 { get; }
        public float PeakValue2 { get; }
        public string PersistedOutputDevice => _deviceManager.GetDefaultEndPoint(ProcessId);
        public int ProcessId { get; }

        private int[] _processIds;
        private int _volume;
        private bool _isMuted;
        private IAudioDeviceManager _deviceManager;

        internal TemporaryAppItemViewModel(IAudioDeviceManager deviceManager, IAppItemViewModel app, bool isChild = false)
        {
            if (!isChild)
            {
                ChildApps = new ObservableCollection<IAppItemViewModel>();
                foreach(var childApp in app.ChildApps)
                {
                    var vm = new TemporaryAppItemViewModel(deviceManager, childApp, true);
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
            Background = new SolidColorBrush(Colors.Red);
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
                _deviceManager.SetDefaultEndPoint(id, pid);
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

        public void RefreshDisplayName() { }
        public void UpdatePeakValueBackground() { }
        public void UpdatePeakValueForeground() { }
    }
}
