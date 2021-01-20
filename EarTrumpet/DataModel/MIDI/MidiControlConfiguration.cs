namespace EarTrumpet.DataModel.MIDI
{
    public class MidiControlConfiguration
    {
        public byte Channel { get; set; }
        public byte Controller { get; set; }
        public byte MinValue { get; set; }
        public byte MaxValue { get; set; }
        public float ScalingValue { get; set; }

        // Constructor
        public MidiControlConfiguration(byte Channel, byte Controller, byte MinValue, byte MaxValue, float ScalingValue)
        {
            this.Channel = Channel;
            this.Controller = Controller;
            this.MinValue = MinValue;
            this.MaxValue = MaxValue;
            this.ScalingValue = ScalingValue;
        }

        public MidiControlConfiguration()
        {
            
        }
    }
}