using System.Linq;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetDefaultDeviceAction : BaseAction, IPartWithDevice
    {
        public Device Device { get; set; }
        
        public SetDefaultDeviceAction()
        {
            Description = Properties.Resources.SetDefaultDeviceActionDescriptionText;
        }

        public override string Describe() => $"Set {Device} as playback default";
    }
}
