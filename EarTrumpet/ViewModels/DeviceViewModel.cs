using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.DataModel.Com;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace EarTrumpet.ViewModels
{
    public class DeviceViewModel : BindableBase
    {
        public AudioSessionViewModel Device { get; private set; }
        public ObservableCollection<AppItemViewModel> Apps { get; private set; }

        IAudioDevice _device;
        FilteredAudioDeviceSessionCollection _sessions;

        public DeviceViewModel(IAudioDevice device)
        {
            _device = device;
            _device.PropertyChanged += Device_PropertyChanged;

            Device = new AudioSessionViewModel(device);
            Apps = new ObservableCollection<AppItemViewModel>();

            UpdateFilterAndEnumerateAppSessions();
        }

        ~DeviceViewModel()
        {
            _device.PropertyChanged -= Device_PropertyChanged;

            if (_sessions != null)
            {
                _sessions.Sessions.CollectionChanged -= Sessions_CollectionChanged;
            }
        }

        void UpdateFilterAndEnumerateAppSessions()
        {
            if (_sessions != null)
            {
                _sessions.Sessions.CollectionChanged -= Sessions_CollectionChanged;
            }

            if (_device.Sessions == null) return;

            _sessions = new FilteredAudioDeviceSessionCollection(_device.Sessions, IsApplicableCheck);
            _sessions.Sessions.CollectionChanged += Sessions_CollectionChanged;

            Apps.Clear();
            foreach (var session in _sessions.Sessions)
            {
                Apps.AddSorted(new AppItemViewModel(session), AppItemViewModelComparer.Instance);
            }
        }

        bool IsApplicableCheck(IAudioDeviceSession session)
        {
            if (session.State == AudioSessionState.Expired)
            {
                return false;
            }

            return true;
        }

        public void TriggerPeakCheck()
        {
            Device.TriggerPeakCheck();

            foreach (var app in Apps) app.TriggerPeakCheck();
        }

        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_device.Sessions))
            {
                UpdateFilterAndEnumerateAppSessions();
            }
        }

        private void Sessions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    var newSession = new AppItemViewModel((IAudioDeviceSession)e.NewItems[0]);
                    Apps.AddSorted(newSession, AppItemViewModelComparer.Instance);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    Apps.Remove(Apps.First(x => x.Id == ((IAudioDeviceSession)e.OldItems[0]).Id));
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    UpdateFilterAndEnumerateAppSessions();
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
            }
        }
    }
}
