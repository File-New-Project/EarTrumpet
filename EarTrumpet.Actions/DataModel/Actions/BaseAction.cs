using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Actions
{
    [XmlInclude(typeof(ChangeAppVolumeAction))]
    [XmlInclude(typeof(ChangeDeviceVolumeAction))]
    [XmlInclude(typeof(SetDefaultDeviceAction))]
   // [XmlInclude(typeof(DelayAction))]
   // [XmlInclude(typeof(TrayIconAction))]
    public abstract class BaseAction : Part
    {
        public abstract void Invoke();
    }
}
