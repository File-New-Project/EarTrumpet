namespace EarTrumpet.DataModel.MIDI
{
    public enum ControllerTypes
    {
        LINEAR_POTENTIOMETER,
        BUTTON,
        ROTARY_ENCODER,
        INVALID_ENTRY
    }

    public class MidiControlConfiguration
    {
        public byte Channel { get; set; }
        public byte Controller { get; set; }
        public ControllerTypes ControllerType { get; set; }
        public byte MinValue { get; set; }
        public byte MaxValue { get; set; }
        public float ScalingValue { get; set; }

        // Constructor
        public MidiControlConfiguration(byte Channel, byte Controller, ControllerTypes ControllerType, byte MinValue, byte MaxValue, float ScalingValue)
        {
            this.Channel = Channel;
            this.Controller = Controller;
            this.ControllerType = ControllerType;
            this.MinValue = MinValue;
            this.MaxValue = MaxValue;
            this.ScalingValue = ScalingValue;
        }

        public MidiControlConfiguration()
        {
            
        }
    }
}