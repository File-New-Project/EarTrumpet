using EarTrumpet.DataModel.Hardware;

namespace EarTrumpet.DataModel.Deej
{
    public class DeejConfiguration: HardwareConfiguration
    {
        public string Port { get; set; }
        public int Channel { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public float ScalingValue { get; set; }

        public DeejConfiguration(string port, int channel, int minValue, int maxValue, float scalingValue)
        {
            Port = port;
            Channel = channel;
            MinValue = minValue;
            MaxValue = maxValue;
            ScalingValue = scalingValue;
        }

        // Default constructor required for serialization.
        public DeejConfiguration()
        {
            
        }
        
        public override string ToString()
        {
            return $"Com Port: {Port}, Channel: {Channel}, Min Value: {MinValue}, Max Value: {MaxValue}," +
                   $"Scaling Value: {ScalingValue}";
        }
    }
}