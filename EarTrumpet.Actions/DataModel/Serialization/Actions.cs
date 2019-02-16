using EarTrumpet.Actions.DataModel.Enum;
using System.Xml.Serialization;

namespace EarTrumpet.Actions.DataModel.Serialization
{
    [XmlInclude(typeof(SetAppVolumeAction))]
    [XmlInclude(typeof(SetAppMuteAction))]
    [XmlInclude(typeof(SetDeviceVolumeAction))]
    [XmlInclude(typeof(SetDeviceMuteAction))]
    [XmlInclude(typeof(SetDefaultDeviceAction))]
    [XmlInclude(typeof(SetVariableAction))]
    public abstract class BaseAction : Part { }

    public class SetAppMuteAction : BaseAction, IPartWithDevice, IPartWithApp
    {
        public Device Device { get; set; }
        public AppRef App { get; set; }
        public MuteKind Option { get; set; }
    }

    public class SetAppVolumeAction : BaseAction, IPartWithVolume, IPartWithDevice, IPartWithApp
    {
        public Device Device { get; set; }
        public AppRef App { get; set; }
        public SetVolumeKind Option { get; set; }
        public double Volume { get; set; }
    }

    public class SetDefaultDeviceAction : BaseAction, IPartWithDevice
    {
        public Device Device { get; set; }
    }

    public class SetDeviceMuteAction : BaseAction, IPartWithDevice
    {
        public Device Device { get; set; }
        public MuteKind Option { get; set; }
    }

    public class SetDeviceVolumeAction : BaseAction, IPartWithDevice, IPartWithVolume
    {
        public Device Device { get; set; }
        public SetVolumeKind Option { get; set; }
        public double Volume { get; set; }
    }

    public class SetVariableAction : BaseAction, IPartWithText
    {
        public string Text { get; set; }
        public BoolValue Value { get; set; }
    }
}
