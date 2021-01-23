using System.Xml.Serialization;
using EarTrumpet.DataModel.MIDI;

namespace EarTrumpet.DataModel.Hardware
{
    [XmlInclude(typeof(MidiControlConfiguration))]
    public abstract class HardwareConfiguration
    {
        public abstract override string ToString();
    }
}