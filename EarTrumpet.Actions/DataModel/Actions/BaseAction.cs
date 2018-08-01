using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Actions
{
    // Ideas: Delay, StartProgram, SetTrayIcon
    [XmlInclude(typeof(SetAppVolumeAction))]
    [XmlInclude(typeof(SetDeviceVolumeAction))]
    [XmlInclude(typeof(SetDefaultDeviceAction))]
    [XmlInclude(typeof(SetVariableAction))]
    [XmlInclude(typeof(SetThemeAction))]
    [XmlInclude(typeof(SetAdditionalSettingsAction))]
    [XmlInclude(typeof(InvokeAddonCommandAction))]
    public abstract class BaseAction : PartWithOptions
    {
    }
}
