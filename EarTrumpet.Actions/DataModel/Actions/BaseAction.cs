using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Actions
{
    [XmlInclude(typeof(SetAppVolumeAction))]
    [XmlInclude(typeof(SetAppMuteAction))]
    [XmlInclude(typeof(SetDeviceVolumeAction))]
    [XmlInclude(typeof(SetDeviceMuteAction))]
    [XmlInclude(typeof(SetDefaultDeviceAction))]
    [XmlInclude(typeof(SetVariableAction))]
    [XmlInclude(typeof(SetThemeAction))]
    [XmlInclude(typeof(SetAdditionalSettingsAction))]
    [XmlInclude(typeof(InvokeAddonCommandAction))]
    public abstract class BaseAction : PartWithOptions
    {
    }
}
