namespace EarTrumpet.ViewModels
{
    public class SimpleAudioDeviceViewModel
    {
        public string DisplayName;
        public string Id;
        public bool IsDefault;

        public override string ToString()
        {
            return DisplayName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is SimpleAudioDeviceViewModel))
                return false;

            return ((SimpleAudioDeviceViewModel)obj).Id == Id;
        }

        public override int GetHashCode()
        {
            if (Id == null) return "Default".GetHashCode();
            return Id.GetHashCode();
        }
    }
}
