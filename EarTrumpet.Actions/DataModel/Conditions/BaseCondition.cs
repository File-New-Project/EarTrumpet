using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Conditions
{
    [XmlInclude(typeof(DefaultPlaybackDeviceCondition))]
    [XmlInclude(typeof(ProcessCondition))]
    [XmlInclude(typeof(TimeCondition))]
    public abstract class BaseCondition : Part
    {
        public abstract bool IsMet();
    }
}
