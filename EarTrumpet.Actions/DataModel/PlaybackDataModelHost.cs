using EarTrumpet.DataModel;
using System;

namespace EarTrumpet_Actions.DataModel
{
    public class PlaybackDataModelHost
    {
        public static IAudioDeviceManager DeviceManager = DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback);

        public static event Action<IAudioDeviceSession, string> AppPropertyChanged;
        public static event Action<IAudioDeviceSession> AppAdded;
        public static event Action<IAudioDeviceSession> AppRemoved;
        public static event Action<IAudioDevice, string> DevicePropertyChanged;
        public static event Action<IAudioDevice> DeviceAdded;
        public static event Action<IAudioDevice> DeviceRemoved;

        public static void InitializeDataModel()
        {
            DeviceManager.Devices.CollectionChanged += Devices_CollectionChanged;

            foreach (var d in DeviceManager.Devices)
            {
                ListenToDevice(d);
            }
        }

        private static void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

        private static void ListenToDevice(IAudioDevice device)
        {
            device.PropertyChanged += Device_PropertyChanged;
            device.Groups.CollectionChanged += Groups_CollectionChanged;

            foreach (var app in device.Groups)
            {
                ListenToApp(app);
            }

            DeviceAdded?.Invoke(device);
        }

        private static void ListenToApp(IAudioDeviceSession app)
        {
            app.PropertyChanged += App_PropertyChanged;
            AppAdded?.Invoke(app);
        }

        private static void App_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AppPropertyChanged?.Invoke((IAudioDeviceSession)sender, e.PropertyName);
        }

        private static void Groups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

        private static void FreeApp(IAudioDeviceSession app)
        {
            app.PropertyChanged -= App_PropertyChanged;
            AppRemoved?.Invoke(app);
        }

        private static void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DevicePropertyChanged?.Invoke((IAudioDevice)sender, e.PropertyName);
        }

        private static void FreeDevice(IAudioDevice device)
        {
            device.PropertyChanged -= Device_PropertyChanged;
            device.Groups.CollectionChanged -= Groups_CollectionChanged;
            DeviceRemoved?.Invoke(device);
        }
    }
}