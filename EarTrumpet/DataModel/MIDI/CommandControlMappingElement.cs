namespace EarTrumpet.DataModel.MIDI
{
    public class CommandControlMappingElement
    {
        public enum Command
        {
            SystemVolume,
            SystemMute,
            ApplicationVolume,
            ApplicationMute,
            None
        };

        public enum Mode
        {
            Indexed,
            ApplicationSelection,
            None
        };
        
        public MidiControlConfiguration midiControlConfiguration { get; set; }
        public string audioDevice { get; set; }
        public Command command { get; set; }
        public Mode mode { get; set; }
        public string indexApplicationSelection { get; set; }
        public string deviceType { get; set; }

        // Constructor
        public CommandControlMappingElement(
            string deviceType,
            MidiControlConfiguration midiControlConfiguration, 
            string audioDevice, 
            Command command,
            Mode mode, 
            string indexApplicationSelection)
        {
            this.deviceType = deviceType;
            this.midiControlConfiguration = midiControlConfiguration;
            this.audioDevice = audioDevice;
            this.command = command;
            this.mode = mode;
            this.indexApplicationSelection = indexApplicationSelection;
        }

        public CommandControlMappingElement()
        {
            
        }
    }
}