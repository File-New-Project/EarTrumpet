namespace EarTrumpet.DataModel.Hardware
{
    public class CommandControlMappingElement
    {
        public enum Command
        {
            SystemVolume,
            SystemMute,
            ApplicationVolume,
            ApplicationMute,
            SetDefaultDevice,
            CycleDefaultDevice,
            None
        };

        public enum Mode
        {
            Indexed,
            ApplicationSelection,
            None
        };
        
        public string audioDevice { get; set; }
        public Command command { get; set; }
        public Mode mode { get; set; }
        public string indexApplicationSelection { get; set; }
        public HardwareConfiguration hardwareConfiguration { get; set; }

        // Constructor
        public CommandControlMappingElement(
            HardwareConfiguration hardwareConfiguration,
            string audioDevice, 
            Command command,
            Mode mode, 
            string indexApplicationSelection)
        {
            this.hardwareConfiguration = hardwareConfiguration;
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