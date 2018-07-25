using System.Linq;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetDefaultDeviceAction : BaseAction, IPartWithDevice
    {
        public Device Device { get; set; }
        
        public SetDefaultDeviceAction()
        {
            Description = "Set a device to be the default playback device";
        }

        public override string Describe() => $"Set {Device} as playback default";
    }
}
