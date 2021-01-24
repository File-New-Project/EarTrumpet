using System.Xml.Serialization;
using EarTrumpet.DataModel.Deej;
using EarTrumpet.DataModel.MIDI;

namespace EarTrumpet.DataModel.Hardware
{
    [XmlInclude(typeof(MidiConfiguration))]
    [XmlInclude(typeof(DeejConfiguration))]
    public abstract class HardwareConfiguration
    {
        public abstract override string ToString();
    }
}