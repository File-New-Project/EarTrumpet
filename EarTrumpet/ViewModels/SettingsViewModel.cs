using EarTrumpet.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        IAudioDeviceManager _manager;

        public ObservableCollection<AudioSessionViewModel> DefaultDevices { get; private set; }
        public ObservableCollection<AudioSessionViewModel> DefaultApps { get; private set; }

        public SettingsViewModel(IAudioDeviceManager manager)
        {
            _manager = manager;

            DefaultDevices = new ObservableCollection<AudioSessionViewModel>();
            DefaultApps = new ObservableCollection<AudioSessionViewModel>();
        }

        public IEnumerable<IAudioDeviceSession> EnumerateApps()
        {
            if (!_manager.VirtualDefaultDevice.IsDevicePresent)
            {
                yield break;
            }

            foreach(var app in _manager.DefaultDevice.Sessions)
            {
                if (!DefaultApps.Any(d => d.Id == app.Id))
                {
                    yield return app;
                }
            }
        }

        public IEnumerable<IAudioDevice> EnumerateDevices()
        {
            if (!_manager.VirtualDefaultDevice.IsDevicePresent)
            {
                yield break;
            }

            foreach (var dev in _manager.Devices)
            {
                if (!DefaultDevices.Any(d => d.Id == dev.Id))
                {
                    yield return dev;
                }
            }
        }

        internal void AddDevice(IAudioDevice dev)
        {
            DefaultDevices.Add(new AudioSessionViewModel(dev));
        }

        internal void AddApp(IAudioDeviceSession app)
        {
            DefaultApps.Add(new AudioSessionViewModel(app));

        }
    }
}
