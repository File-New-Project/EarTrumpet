using System.Xml.Serialization;

namespace EarTrumpet.DataModel.MIDI
{
    [XmlInclude(typeof(MidiControlConfiguration))]
    public abstract class HardwareConfiguration
    {
        public abstract override string ToString();
        public abstract void FromString(string str);
    }
}