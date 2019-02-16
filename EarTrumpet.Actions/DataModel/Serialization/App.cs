namespace EarTrumpet.Actions.DataModel.Serialization
{
    public class AppRef
    {
        public static readonly string EveryAppId = "EarTrumpet.EveryApp";
        public static readonly string ForegroundAppId = "EarTrumpet.ForegroundApp";

        public string Id { get; set; }

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.GetHashCode();
        }

        public bool Equals(AppRef other)
        {
            return other.Id == Id;
        }
    }
}