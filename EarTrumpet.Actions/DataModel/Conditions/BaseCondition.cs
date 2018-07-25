using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Conditions
{
    [XmlInclude(typeof(DefaultPlaybackDeviceCondition))]
    [XmlInclude(typeof(ProcessCondition))]
    [XmlInclude(typeof(VariableCondition))]
    public abstract class BaseCondition : PartWithOptions
    {
    }
}
