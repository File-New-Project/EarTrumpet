using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using System;

namespace EarTrumpet.Extensibility.Shared
{
    public class PlaybackDataModelHost
    {
        public static PlaybackDataModelHost Current { get; } = new PlaybackDataModelHost();

        public IAudioDeviceManager DeviceManager = WindowsAudioFactory.Create(AudioDeviceKind.Playback);

        public event Action<IAudioDeviceSession, string> AppPropertyChanged;
        public event Action<IAudioDeviceSession> AppAdded;
        public event Action<IAudioDeviceSession> AppRemoved;
        public event Action<IAudioDevice, string> DevicePropertyChanged;
        public event Action<IAudioDevice> DeviceAdded;
        public event Action<IAudioDevice> DeviceRemoved;

        private PlaybackDataModelHost()
        {
            DeviceManager.Devices.CollectionChanged += Devices_CollectionChanged;

            foreach (var d in DeviceManager.Devices)
            {
                ListenToDevice(d);
            }
        }

        private void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    ListenToDevice((IAudioDevice)e.NewItems[0]);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    FreeDevice((IAudioDevice)e.OldItems[0]);
                    break;
                default: throw new NotImplementedException();
            }
        }

        private void ListenToDevice(IAudioDevice device)
        {
            device.PropertyChanged += Device_PropertyChanged;
            device.Groups.CollectionChanged += Groups_CollectionChanged;

            foreach (var app in device.Groups)
            {
                ListenToApp(app);
            }

            DeviceAdded?.Invoke(device);
        }

        private void ListenToApp(IAudioDeviceSession app)
        {
            app.PropertyChanged += App_PropertyChanged;
            AppAdded?.Invoke(app);
        }

        private void App_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AppPropertyChanged?.Invoke((IAudioDeviceSession)sender, e.PropertyName);
        }

        private void Groups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    ListenToApp((IAudioDeviceSession)e.NewItems[0]);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    FreeApp((IAudioDeviceSession)e.OldItems[0]);
                    break;
                default: throw new NotImplementedException();
            }
        }

        private void FreeApp(IAudioDeviceSession app)
        {
            app.PropertyChanged -= App_PropertyChanged;
            AppRemoved?.Invoke(app);
        }

        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DevicePropertyChanged?.Invoke((IAudioDevice)sender, e.PropertyName);
        }

        private void FreeDevice(IAudioDevice device)
        {
            device.PropertyChanged -= Device_PropertyChanged;
            device.Groups.CollectionChanged -= Groups_CollectionChanged;
            DeviceRemoved?.Invoke(device);
        }
    }
}