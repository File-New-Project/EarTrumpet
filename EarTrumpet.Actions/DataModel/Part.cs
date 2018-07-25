using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel
{
    public abstract class Part
    {
        public abstract string Describe();

        [XmlIgnore]
        public string Description { get; protected set; }

        public virtual void Loaded()
        {

        }
    }
}
