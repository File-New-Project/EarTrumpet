using EarTrumpet.DataModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet_Actions.DataModel.Serialization
{
    public class Device
    {

        public Device()
        {

        }

        public Device(IAudioDevice device)
        {
            Id = device.Id;
            Kind = device.Parent.DeviceKind;
        }

        public string Id { get; set; }

        public AudioDeviceKind Kind { get; set; }

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.GetHashCode();
        }
    }
}
