namespace EarTrumpet.DataModel.MIDI
{
    public class CommandControlMappingElement
    {
        MidiControlConfiguration midiControlConfiguration { get; set; }
        string audioDevice { get; set; }
        string command { get; set; }

        string mode { get; set; }

        string indexApplicationSelection { get; set; }

        string midiDevice { get; set; }

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