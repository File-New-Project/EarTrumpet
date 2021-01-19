namespace EarTrumpet.DataModel.MIDI
{
    public class CommandControlMappingElement
    {
        public MidiControlConfiguration midiControlConfiguration { get; set; }
        public string audioDevice { get; set; }
        public string command { get; set; }

        public string mode { get; set; }

        public string indexApplicationSelection { get; set; }

        public string midiDevice { get; set; }

        // Constructor
        public CommandControlMappingElement(
            MidiControlConfiguration midiControlConfiguration, 
            string audioDevice, 
            string command,
            string mode, 
            string indexApplicationSelection, 
            string midiDevice)
        {
            this.midiControlConfiguration = midiControlConfiguration;
            this.audioDevice = audioDevice;
            this.command = command;
            this.mode = mode;
            this.indexApplicationSelection = indexApplicationSelection;
            this.midiDevice = midiDevice;
        }
    }
}