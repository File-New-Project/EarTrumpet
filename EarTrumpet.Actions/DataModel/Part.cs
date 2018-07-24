using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel
{
    public abstract class Part
    {
        [XmlIgnore]
        public object Option { get; set; }
        [XmlIgnore]
        public List<Option> Options { get; protected set; }
        [XmlIgnore]
        public string DisplayName { get; set; }

        public abstract void Loaded();
    }
}
