namespace EarTrumpet_Actions.DataModel.Serialization
{
    public class App
    {
        public static readonly string EveryAppId = "EarTrumpet.EveryApp";
        public static readonly string ForegroundAppId = "EarTrumpet.ForegroundApp";

        public string Id { get; set; }

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.GetHashCode();
        }

        public bool Equals(App other)
        {
            return other.Id == Id;
        }
    }
}