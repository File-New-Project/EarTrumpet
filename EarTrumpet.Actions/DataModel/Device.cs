using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EarTrumpet.DataModel;

namespace EarTrumpet_Actions.DataModel
{
    public class Device
    {
        public static Device AnyDevice = new Device { Id = "ANY_DEVICE_ID" };

        public static ObservableCollection<Option> AllDevices
        {
            get
            {
                var ret = new ObservableCollection<Option>();
                ret.Add(new Option("Default playback device", null));
                ret.Add(new Option("Any playback device", AnyDevice));
                foreach (var device in PlaybackDataModelHost.DeviceManager.Devices)
                {
                    ret.Add(new Option(device.DisplayName, new Device(device)));
                }
                return ret;
            }
        }

        public Device()
        {

        }

        public Device(IAudioDevice device)
        {
            Id = device.Id;
        }

        public string Id { get; set; }

        public override string ToString()
        {
            if (Id == null)
            {
                return "Default device";
            }
            if (Id == AnyDevice.Id)
            {
                return "any device";
            }

            var device = PlaybackDataModelHost.DeviceManager.Devices.FirstOrDefault(d => d.Id == Id);
            if (device != null)
            {
                return device.DisplayName;
            }
            return Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
