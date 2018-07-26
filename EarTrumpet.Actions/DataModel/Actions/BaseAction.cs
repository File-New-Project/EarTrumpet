using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Actions
{
    // Ideas: Delay, StartProgram, SetTrayIcon
    [XmlInclude(typeof(ChangeAppVolumeAction))]
    [XmlInclude(typeof(ChangeDeviceVolumeAction))]
    [XmlInclude(typeof(SetDefaultDeviceAction))]
    [XmlInclude(typeof(SetVariableAction))]
    [XmlInclude(typeof(SetThemeAction))]
    [XmlInclude(typeof(SetAddonEarTrumpetSettingsAction))]
    public abstract class BaseAction : PartWithOptions
    {

    }
}
