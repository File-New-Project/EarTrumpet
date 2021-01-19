namespace EarTrumpet.DataModel.MIDI
{
    public class MidiControlConfiguration
    {
        byte Channel { get; set; }
        byte Controller { get; set; }
        byte MinValue { get; set; }
        byte MaxValue { get; set; }
        float ScalingValue { get; set; }

        // Constructor
        public MidiControlConfiguration(byte Channel, byte Controller, byte MinValue, byte MaxValue, float ScalingValue)
        {
            this.Channel = Channel;
            this.Controller = Controller;
            this.MinValue = MinValue;
            this.MaxValue = MaxValue;
            this.ScalingValue = ScalingValue;
        }
    }
}