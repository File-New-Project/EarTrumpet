using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetDefaultDeviceAction : BaseAction
    {
        public Device Device { get; set; }
        
        public SetDefaultDeviceAction()
        {
            DisplayName = "Set a device to be the default playback device";
        }

        public override void Invoke()
        {
            var dev = PlaybackDataModelHost.DeviceManager.Devices.FirstOrDefault(d => d.Id == Device.Id);
            if (dev != null)
            {
                PlaybackDataModelHost.DeviceManager.Default = dev;
            }
        }

        public override void Loaded()
        {
            DisplayName = $"Set {Device} as playback default";
        }
    }
}
