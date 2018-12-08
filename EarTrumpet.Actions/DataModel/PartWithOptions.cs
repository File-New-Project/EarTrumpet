using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel
{
    public class PartWithOptions : Part
    {
        public override string Describe()
        {
            throw new NotImplementedException();
        }

        [XmlIgnore]
        public List<OptionCollection> Options { get; protected set; }
    }
}
